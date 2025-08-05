using Npgsql;
using UserManagementApi.Services.Interfaces;
using Microsoft.Extensions.Configuration;
 
namespace UserManagementApi.Services.Implementations
{
    public class OtpService : IOtpService
    {
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
 
        public OtpService(IConfiguration configuration, IEmailService emailService)
        {
            _configuration = configuration;
            _emailService = emailService;
        }
 
       public async Task SendOtpEmailAsync(string email)
        {
            var otpCode = GenerateOtp();
            var expiry = DateTime.UtcNow.AddMinutes(5);

            await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            var updateCmd = new NpgsqlCommand(@"
                UPDATE public.users 
                SET otpcode = @Otp, 
                    ""otpExpiry"" = @Expiry 
                WHERE email = @Email", conn);

            updateCmd.Parameters.AddWithValue("Otp", otpCode);
            updateCmd.Parameters.AddWithValue("Expiry", expiry);
            updateCmd.Parameters.AddWithValue("Email", email);

            await updateCmd.ExecuteNonQueryAsync();

            await _emailService.SendOtpEmail(email, otpCode);
        }

 
        public async Task<bool> VerifyOtpAsync(string email, string otpCode)
        {
            await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();
        
            var cmd = new NpgsqlCommand(@"SELECT otpcode, ""otpExpiry"" 
                                  FROM public.users 
                                  WHERE email = @Email;", conn);
            cmd.Parameters.AddWithValue("Email", email);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (!reader.HasRows)
                return false;

            await reader.ReadAsync();

            var storedOtp = reader["otpcode"]?.ToString();
            var expiry = reader["otpExpiry"] == DBNull.Value 
                ? (DateTime?)null 
                : Convert.ToDateTime(reader["otpExpiry"]);

            reader.Close();

            if (storedOtp == otpCode && expiry.HasValue && expiry.Value > DateTime.UtcNow)
            {
                await ClearOtpAsync(email); 
                return true;
            }
        
            return false;
        }
        
        private async Task ClearOtpAsync(string email)
        {
            await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            var clearCmd = new NpgsqlCommand(@"
                UPDATE public.users 
                SET otpcode = NULL, ""otpExpiry"" = NULL 
                WHERE email = @Email;", conn);

            clearCmd.Parameters.AddWithValue("Email", email);

            await clearCmd.ExecuteNonQueryAsync();
        }
 
        private string GenerateOtp()
        {
            var randomNumber = new Random();
            return randomNumber.Next(100000, 999999).ToString(); 
        }
    }
}