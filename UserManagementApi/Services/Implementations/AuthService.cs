using Npgsql;
using UserManagementApi.Exceptions;
using UserManagementApi.Helper;
using UserManagementApi.Services.Interfaces;
using UserManagementApi.ViewModels;

namespace UserManagementApi.Services.Implementations;

public class AuthService:IAuthService
{

     private readonly IConfiguration _configuration;
     private readonly IEmailService _emailService;


    public AuthService(IConfiguration configuration, IEmailService emailService)
    {
        _configuration = configuration;
        _emailService = emailService;
    }

    public async Task<UserViewModel?> AuthenticateUserAsync(string email, string password)
    {
        var user = await GetUserByEmailAsync(email);

        if (user != null && PasswordHasher.VerifyPassword(password, user.PasswordHash))
        {
            return user;
        }
        return null;
    }

    public async Task<(bool Success, string Token, int UserId, string Message)> ForgotPasswordAsync(string email, string baseUrl)
    {
        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        var cmd = new NpgsqlCommand("SELECT id, email FROM public.users WHERE email = @Email AND \"Isdeleted\" = false", conn);
        cmd.Parameters.AddWithValue("Email", email);

        var reader = await cmd.ExecuteReaderAsync();

        if (!reader.HasRows)
            throw new NotFoundException("User with the given email does not exist.");

        await reader.ReadAsync();
        var userId = reader.GetInt32(0);
        reader.Close();

        var token = Guid.NewGuid().ToString();
        var expiry = DateTime.UtcNow.AddHours(1);

        var updateCmd = new NpgsqlCommand("UPDATE public.users SET \"PasswordResetToken\" = @Token, \"ResetTokenExpiry\" = @Expiry WHERE id = @UserId", conn);
        updateCmd.Parameters.AddWithValue("Token", token);
        updateCmd.Parameters.AddWithValue("Expiry", expiry);
        updateCmd.Parameters.AddWithValue("UserId", userId);
        await updateCmd.ExecuteNonQueryAsync();

        var resetLink = $"{baseUrl}/Account/ResetPassword?userId={userId}&token={token}";
        Console.WriteLine("Reset Password Link: " + resetLink);

         await _emailService.SendPasswordResetEmail(email, resetLink);

        return (true, token, userId, "A password reset link has been generated.");
    }

    public async Task<UserViewModel?> GetUserByEmailAsync(string email)
    {
        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("SELECT * FROM get_user_by_email(@user_email)", conn);
        cmd.Parameters.AddWithValue("user_email", email);

        await using var reader = await cmd.ExecuteReaderAsync();

        if (!reader.HasRows)
            return null;

        UserViewModel? user = null;

        while (await reader.ReadAsync())
        {
            user = new UserViewModel
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Firstname = reader.GetString(reader.GetOrdinal("firstname")),
                Lastname = reader.GetString(reader.GetOrdinal("lastname")),
                Email = reader.GetString(reader.GetOrdinal("email")),
                PhoneNumber = reader.GetInt64(reader.GetOrdinal("phonenumber")),
                RoleId = reader.GetInt32(reader.GetOrdinal("roleid")),
                RoleName = reader.GetString(reader.GetOrdinal("role_name")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("isactive")),
                PasswordHash = reader.GetString(reader.GetOrdinal("passwordhash"))
            };
        }

        return user;
    }

    public async Task<bool> ValidateResetTokenAsync(int userId, string token)
    {
        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        var cmd = new NpgsqlCommand(@"SELECT ""PasswordResetToken"", ""ResetTokenExpiry"" 
                                    FROM public.users 
                                    WHERE id = @UserId AND ""Isdeleted"" = false", conn);
        cmd.Parameters.AddWithValue("UserId", userId);

        var reader = await cmd.ExecuteReaderAsync();

        if (!reader.HasRows)
            return false;

        await reader.ReadAsync();

        var storedToken = reader["PasswordResetToken"]?.ToString();
        var expiry = Convert.ToDateTime(reader["ResetTokenExpiry"]);

        return storedToken == token && DateTime.UtcNow <= expiry;
    }

    public async Task<string> ResetPasswordAsync(int userId, string token, string newPassword)
    {
        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        var cmd = new NpgsqlCommand(@"SELECT ""PasswordResetToken"", ""ResetTokenExpiry"" 
                                    FROM public.users 
                                    WHERE id = @UserId AND ""Isdeleted"" = false", conn);
        cmd.Parameters.AddWithValue("UserId", userId);

        var reader = await cmd.ExecuteReaderAsync();
        if (!reader.HasRows)
            throw new NotFoundException("The requested user could not be found.");

        await reader.ReadAsync();

        var storedToken = reader["PasswordResetToken"]?.ToString();
        var expiry = Convert.ToDateTime(reader["ResetTokenExpiry"]);

        if (storedToken != token || DateTime.UtcNow > expiry)
            throw new ValidationException("Token is invalid or expired. Please request a new one.");

        reader.Close();

        var hashedPassword = PasswordHasher.HashPassword(newPassword);

        var updateCmd = new NpgsqlCommand(@"UPDATE public.users 
                                            SET password = @Password, 
                                                ""PasswordHash"" = @Hashed, 
                                                ""PasswordResetToken"" = NULL,
                                                ""ResetTokenExpiry"" = NULL
                                            WHERE id = @UserId", conn);
        updateCmd.Parameters.AddWithValue("Password", newPassword);
        updateCmd.Parameters.AddWithValue("Hashed", hashedPassword);
        updateCmd.Parameters.AddWithValue("UserId", userId);

        await updateCmd.ExecuteNonQueryAsync();

        return "Your password has been reset successfully.";
    }

}
