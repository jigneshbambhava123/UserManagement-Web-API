using Npgsql;
using UserManagementApi.Helper;
using UserManagementApi.Services.Interfaces;
using UserManagementApi.ViewModels;
using UserManagementApi.Exceptions;
using Microsoft.AspNetCore.SignalR;


namespace UserManagementApi.Services.Implementations;

public class UserService : IUserService
{
    private readonly IConfiguration _configuration;
    private readonly IHubContext<ResourceHub> _hubContext;

    public UserService(IConfiguration configuration, IHubContext<ResourceHub> hubContext)
    {
        _configuration = configuration;
        _hubContext = hubContext;
    }

    public async Task CreateUser(UserViewModel userViewModel)
    {
        if (await EmailExists(userViewModel.Email))
        {   
            throw new ValidationException("This email is linked to another user."); 
        }

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

        var countCmd = new NpgsqlCommand("SELECT COUNT(*) FROM public.users WHERE \"Isdeleted\"=false", conn);
        var countResult = await countCmd.ExecuteScalarAsync();
        int activeUserCount = countResult != null ? Convert.ToInt32(countResult) : 0;

        await _hubContext.Clients.All.SendAsync("ReceiveUserCountUpdate", activeUserCount);
    }

    public async Task UpdateUser(UserViewModel userViewModel)
    {
        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        var existsCmd = new NpgsqlCommand("SELECT 1 FROM public.users WHERE id=@id AND \"Isdeleted\"=false", conn);
        existsCmd.Parameters.AddWithValue("id", userViewModel.Id);
        var exists = await existsCmd.ExecuteScalarAsync() != null;
        if (!exists)
        {
            throw new NotFoundException($"User with ID {userViewModel.Id} not found.");
        }

        if (await EmailExists(userViewModel.Email, userViewModel.Id))
        {
            throw new ValidationException("This email is linked to another user."); 
        }

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
    }

    public async Task DeleteUser(int id) 
    {
        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        var existsCmd = new NpgsqlCommand("SELECT 1 FROM public.users WHERE id=@id AND \"Isdeleted\"=false", conn);
        existsCmd.Parameters.AddWithValue("id", id);
        if (await existsCmd.ExecuteScalarAsync() == null)
        {
            throw new NotFoundException($"User with ID {id} not found for deletion.");
        }

        await using var cmd = new NpgsqlCommand("CALL public.delete_user(@p_id)", conn);
        cmd.Parameters.AddWithValue("p_id", id);

        await cmd.ExecuteNonQueryAsync();

        var countCmd = new NpgsqlCommand("SELECT COUNT(*) FROM public.users WHERE \"Isdeleted\"=false", conn);
        var countResult = await countCmd.ExecuteScalarAsync();
        int activeUserCount = countResult != null ? Convert.ToInt32(countResult) : 0;

        await _hubContext.Clients.All.SendAsync("ReceiveUserCountUpdate", activeUserCount);
    }

    public async Task<(List<UserViewModel>, int)> GetUsers(string? search, string? sortColumn, string? sortDirection, int pageNumber, int pageSize)
    {
        var users = new List<UserViewModel>();
        int totalCount = 0;

        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        var query = "SELECT * FROM public.get_all_users_filtered(@search, @sortColumn, @sortDirection, @pageNumber, @pageSize)";
        await using var cmd = new NpgsqlCommand(query, conn);

        cmd.Parameters.AddWithValue("@search", search ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@sortColumn", sortColumn ?? "firstname");
        cmd.Parameters.AddWithValue("@sortDirection", sortDirection ?? "asc");
        cmd.Parameters.AddWithValue("@pageNumber", pageNumber);
        cmd.Parameters.AddWithValue("@pageSize", pageSize);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var user = new UserViewModel
            {
                Id = reader.GetInt32(0),
                Firstname = reader.GetString(1),
                Lastname = reader.GetString(2),
                Email = reader.GetString(3),
                Password = reader.GetString(4),
                RoleId = reader.GetInt32(5),
                PhoneNumber = reader.GetInt64(6),
                IsActive = reader.GetBoolean(7),
                Dateofbirth = reader.GetDateTime(8),
                RoleName = reader.IsDBNull(9) ? string.Empty : reader.GetString(9)
            };

            totalCount = reader.GetInt32(10); 
            users.Add(user);
        }

        return (users, totalCount);
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
        throw new NotFoundException($"User with ID {id} not found.");
    }
}