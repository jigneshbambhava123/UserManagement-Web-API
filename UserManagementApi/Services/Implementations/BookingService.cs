using System.Data;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using UserManagementApi.Helper;
using UserManagementApi.Services.Interfaces;
using UserManagementApi.ViewModels;
using UserManagementApi.Exceptions; 

namespace UserManagementApi.Services.Implementations;

public class BookingService : IBookingService
{
    private readonly IConfiguration _configuration;
    private readonly IHubContext<ResourceHub> _hubContext;
    private readonly IResourceService _resourceService; 

    public BookingService(IConfiguration configuration, IHubContext<ResourceHub> hubContext, IResourceService resourceService)
    {
        _configuration = configuration;
        _hubContext = hubContext;
        _resourceService = resourceService;
    }

    public async Task CreateBooking(BookingViewModel booking)
    {
        if (booking.FromDate.Date < DateTime.Today.Date)
        {
            throw new ValidationException("Booking from date cannot be in the past.");
        }
        if (booking.ToDate.Date < booking.FromDate.Date)
        {
            throw new ValidationException("Booking to date cannot be before from date.");
        }
        if (booking.Quantity <= 0)
        {
            throw new ValidationException("Booking quantity must be positive.");
        }

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

        bool success = (bool)vSuccess.Value;
        string message = vMessage.Value?.ToString() ?? "Unknown error occurred.";

        if (!success)
        {
            throw new ValidationException(message);
        }

        ResourceViewModel? resource = null;
        try
        {
            resource = await _resourceService.GetResourceById(booking.ResourceId);
        }
        catch (NotFoundException) 
        {
            throw new ValidationException($"Resource with ID {booking.ResourceId} not found.");
        }

        int newAvailableQty = resource.Quantity - (resource.UsedQuantity ?? 0); 
        await _hubContext.Clients.All.SendAsync("ReceiveQuantityUpdate", booking.ResourceId, newAvailableQty);
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

    public async Task ReleaseExpiredBookings()
    {
        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("CALL public.release_expired_bookings()", conn);
        await cmd.ExecuteNonQueryAsync();
    }
}