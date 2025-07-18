using UserManagementApi.ViewModels;
using UserManagementApi.Services.Interfaces;
using Npgsql;
using UserManagementApi.Exceptions; 


namespace UserManagementApi.Services.Implementations;


public class DashboardService : IDashboardService
{
    private readonly IConfiguration _configuration;

    public DashboardService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<int> GetActiveUsersCount()
    {
        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("SELECT public.get_active_users_count()", conn);
        var result = await cmd.ExecuteScalarAsync();
        return result != null ? Convert.ToInt32(result) : 0;
    }

    public async Task<List<DailyResourceUsageViewModel>> GetDailyResourceUsage(int userId, int days = 30)
    {
        var data = new List<DailyResourceUsageViewModel>();
        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("SELECT * FROM public.get_daily_resource_usage(@p_userid, @p_days)", conn);
        cmd.Parameters.AddWithValue("p_userid", userId);
        cmd.Parameters.AddWithValue("p_days", days);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            data.Add(new DailyResourceUsageViewModel
            {
                BookingDate = reader.GetDateTime(reader.GetOrdinal("booking_date")),
                TotalActiveQuantity = reader.GetInt64(reader.GetOrdinal("total_active_quantity")),
                TotalUsedQuantity = reader.GetInt64(reader.GetOrdinal("total_used_quantity"))
            });
        }
        return data;
    }

}
