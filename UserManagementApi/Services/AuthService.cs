using Npgsql;
using UserManagementApi.Helper;
using UserManagementApi.ViewModels;

namespace UserManagementApi.Services;

public class AuthService:IAuthService
{

     private readonly IConfiguration _configuration;

    public AuthService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<User?> AuthenticateUserAsync(string email, string password)
    {
        var user = await GetUserByEmailAsync(email);

        if (user != null && PasswordHasher.VerifyPassword(password, user.PasswordHash))
        {
            return user;
        }

        return null;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("SELECT * FROM get_user_by_email(@user_email)", conn);
        cmd.Parameters.AddWithValue("user_email", email);

        await using var reader = await cmd.ExecuteReaderAsync();

        if (!reader.HasRows)
            return null;

        User? user = null;

        while (await reader.ReadAsync())
        {
            user = new User
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



}
