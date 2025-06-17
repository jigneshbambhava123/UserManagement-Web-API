using System.Data;
using Npgsql;
using UserManagementApi.Services.Interfaces;
using UserManagementApi.ViewModels;

namespace UserManagementApi.Services.Implementations;

public class BookingService : IBookingService
{
    private readonly IConfiguration _configuration;

    public BookingService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<(bool Success, string Message)> CreateBooking(BookingViewModel booking)
    {
        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("create_booking", conn)
        {
            CommandType = CommandType.StoredProcedure 
        };

        cmd.Parameters.AddWithValue("p_resourceid", booking.ResourceId);
        cmd.Parameters.AddWithValue("p_userid", booking.UserId);
        cmd.Parameters.Add("p_fromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = booking.FromDate.Date;
        cmd.Parameters.Add("p_todate", NpgsqlTypes.NpgsqlDbType.Date).Value = booking.ToDate.Date;
        cmd.Parameters.AddWithValue("p_quantity", booking.Quantity);

        var vSuccess = new NpgsqlParameter("v_success", NpgsqlTypes.NpgsqlDbType.Boolean)
        {
            Direction = ParameterDirection.Output
        };
        var vMessage = new NpgsqlParameter("v_message", NpgsqlTypes.NpgsqlDbType.Text)
        {
            Direction = ParameterDirection.Output
        };

        cmd.Parameters.Add(vSuccess);
        cmd.Parameters.Add(vMessage);

        await cmd.ExecuteNonQueryAsync();

        return ((bool)vSuccess.Value, vMessage.Value?.ToString() ?? "");
    }

    public async Task<List<BookingViewModel>> GetBookingHistory(int? userId = null)
    {
        var bookings = new List<BookingViewModel>();
        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("SELECT * FROM public.get_resource_booking_history(@p_userid)", conn);
        cmd.Parameters.AddWithValue("p_userid", (object?)userId ?? DBNull.Value);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            bookings.Add(new BookingViewModel
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                UserId = reader.GetInt32(reader.GetOrdinal("userid")),
                ResourceId = reader.GetInt32(reader.GetOrdinal("resourceid")),
                ResourceName = reader.IsDBNull(reader.GetOrdinal("resourcename")) ? null : reader.GetString(reader.GetOrdinal("resourcename")),
                Quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
                FromDate = reader.GetDateTime(reader.GetOrdinal("fromdate")),
                ToDate = reader.GetDateTime(reader.GetOrdinal("todate"))
            });
        }
        return bookings;
    }

    public async Task<List<BookingViewModel>> GetActiveBookings(int? userId = null)
    {
        var bookings = new List<BookingViewModel>();
        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("SELECT * FROM public.get_active_resource_bookings(@p_userid)", conn);
        cmd.Parameters.AddWithValue("p_userid", (object?)userId ?? DBNull.Value);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            bookings.Add(new BookingViewModel
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                UserId = reader.GetInt32(reader.GetOrdinal("userid")),
                ResourceId = reader.GetInt32(reader.GetOrdinal("resourceid")),
                ResourceName = reader.IsDBNull(reader.GetOrdinal("resourcename")) ? null : reader.GetString(reader.GetOrdinal("resourcename")),
                Quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
                FromDate = reader.GetDateTime(reader.GetOrdinal("fromdate")),
                ToDate = reader.GetDateTime(reader.GetOrdinal("todate"))
            });
        }
        return bookings;
    }

    public async Task<(bool Success, string Message)> ReleaseExpiredBookings()
    {
        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("CALL public.release_expired_bookings()", conn);
        await cmd.ExecuteNonQueryAsync();

        return (true, "Expired bookings released successfully.");
    }

}
