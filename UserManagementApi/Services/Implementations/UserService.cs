using Npgsql;
using UserManagementApi.Helper;
using UserManagementApi.Services.Interfaces;
using UserManagementApi.ViewModels;

namespace UserManagementApi.Services.Implementations;

public class UserService : IUserService
{

    private readonly IConfiguration _configuration;

    public UserService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<(bool Success, string Message)> CreateUser(UserViewModel userViewModel)
    {
        if (await EmailExists(userViewModel.Email))
            return (false, "This email is linked to another user.");

        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("CALL public.create_user(@p_firstname, @p_lastname, @p_email, @p_password, @p_roleid, @p_phonenumber, @p_passwordhash, @p_dateofbirth::date)", conn);
        
        cmd.Parameters.AddWithValue("p_firstname", userViewModel.Firstname);
        cmd.Parameters.AddWithValue("p_lastname", userViewModel.Lastname);
        cmd.Parameters.AddWithValue("p_email", userViewModel.Email);
        cmd.Parameters.AddWithValue("p_password", userViewModel.Password);
        cmd.Parameters.AddWithValue("p_roleid", userViewModel.RoleId);
        cmd.Parameters.AddWithValue("p_phonenumber", userViewModel.PhoneNumber);
        cmd.Parameters.AddWithValue("p_passwordhash", PasswordHasher.HashPassword(userViewModel.Password));
        cmd.Parameters.AddWithValue("p_dateofbirth", userViewModel.Dateofbirth.Date); 

        await cmd.ExecuteNonQueryAsync();
        return (true, "User created successfully.");
    }



    public async Task<(bool Success, string Message)> UpdateUser(UserViewModel userViewModel)
    {
        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        var existsCmd = new NpgsqlCommand("SELECT 1 FROM public.users WHERE id=@id AND \"Isdeleted\"=false", conn);
        existsCmd.Parameters.AddWithValue("id", userViewModel.Id);
        var exists = await existsCmd.ExecuteScalarAsync() != null;
        if (!exists)
            return (false, $"User with ID {userViewModel.Id} not found.");

        if (await EmailExists(userViewModel.Email, userViewModel.Id))
            return (false, "This email is linked to another user.");

        await using var cmd = new NpgsqlCommand("CALL public.update_user(@p_id, @p_firstname, @p_lastname, @p_email, @p_password, @p_roleid, @p_phonenumber, @p_isactive)", conn);
        cmd.Parameters.AddWithValue("p_id", userViewModel.Id);
        cmd.Parameters.AddWithValue("p_firstname", userViewModel.Firstname);
        cmd.Parameters.AddWithValue("p_lastname", userViewModel.Lastname);
        cmd.Parameters.AddWithValue("p_email", userViewModel.Email);
        cmd.Parameters.AddWithValue("p_password", userViewModel.Password);
        cmd.Parameters.AddWithValue("p_roleid", userViewModel.RoleId);
        cmd.Parameters.AddWithValue("p_phonenumber", userViewModel.PhoneNumber);
        cmd.Parameters.AddWithValue("p_isactive", userViewModel.IsActive);

        await cmd.ExecuteNonQueryAsync();
        return (true, "User updated successfully.");
    }

    public async Task<(bool Success, string Message)> DeleteUser(int id)
    {
        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        // Proceed to delete
        await using var cmd = new NpgsqlCommand("CALL public.delete_user(@p_id)", conn);
        cmd.Parameters.AddWithValue("p_id", id);

        await cmd.ExecuteNonQueryAsync();
        return (true, "User deleted successfully.");
    }


    public async Task<IEnumerable<UserViewModel>> GetUsers()
    {
        var users = new List<UserViewModel>();
        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        var query = "SELECT * FROM public.get_all_users()";
        await using var cmd = new NpgsqlCommand(query, conn);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            users.Add(new UserViewModel
            {
                Id = reader.GetInt32(0),
                Firstname = reader.GetString(1),
                Lastname = reader.GetString(2),
                Email = reader.GetString(3),
                Password = reader.GetString(4),
                RoleId = reader.GetInt32(5),
                PhoneNumber = reader.GetInt64(6),
                IsActive = reader.GetBoolean(7),
                Dateofbirth = reader.GetDateTime(8) 
            });
        }
        return users;
    }

    public async Task<bool> EmailExists(string email, int? excludeUserId = null)
    {
        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        var sql = "SELECT 1 FROM public.users WHERE email = @p_email AND \"Isdeleted\" = false";
        if (excludeUserId.HasValue)
            sql += " AND id <> @p_id";
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("p_email", email);
        if (excludeUserId.HasValue)
            cmd.Parameters.AddWithValue("p_id", excludeUserId.Value);

        var exists = await cmd.ExecuteScalarAsync() != null;
        return exists;
    }

    public async Task<UserViewModel?> GetUserById(int id)
    {
        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        var query = "SELECT * FROM public.get_user_by_id(@p_id)";
        await using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("p_id", id);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new UserViewModel
            {
                Id = reader.GetInt32(0),
                Firstname = reader.GetString(1),
                Lastname = reader.GetString(2),
                Email = reader.GetString(3),
                Password = reader.GetString(4),
                RoleId = reader.GetInt32(5),
                PhoneNumber = reader.GetInt64(6),
                IsActive = reader.GetBoolean(7)
            };
        }
        return null;
    }

}
