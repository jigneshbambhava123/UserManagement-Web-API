using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementApi.Services.Interfaces;
using UserManagementApi.ViewModels;
using UserManagementApi.Filters; 

namespace UserManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [Authorize(Roles = "Admin,User")]
    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] BookingViewModel booking)
    {
        await _bookingService.CreateBooking(booking);
        return Ok("Booking created successfully.");
    }

    [Authorize(Roles = "Admin,User")]
    [HttpGet("ResourceHistory")]
    public async Task<IActionResult> GetResourceHistory(
        [FromQuery] int? userId = null,
        [FromQuery] string? search = null,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortDirection = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? timeFilter = null 
    )
    {
        var (bookings, totalCount) = await _bookingService.GetBookingHistoryFilteredAsync(
            userId, search, sortColumn, sortDirection, pageNumber, pageSize, timeFilter);

        return Ok(new
        {
            data = bookings,
            totalCount = totalCount
        });
    }


    [Authorize(Roles = "Admin,User")]
    [HttpGet("ActiveBookings")]
    public async Task<IActionResult> GetActiveBookings(
        [FromQuery] int? userId = null,
        [FromQuery] string? search = null,
        [FromQuery] string? sortColumn = "todate",
        [FromQuery] string? sortDirection = "desc",
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? timeFilter = null)
    {
        var (bookings, totalCount) = await _bookingService.GetActiveBookingsFilteredAsync(
            userId, search, sortColumn, sortDirection, pageNumber, pageSize, timeFilter);

        return Ok(new
        {
            data = bookings,
            totalCount = totalCount
        });
    }

    [HttpPost("ReleaseExpiredBookings")]
    public async Task<IActionResult> ReleaseExpiredBookings()
    {
        await _bookingService.ReleaseExpiredBookings();
        return Ok("Expired bookings released successfully.");
    }
}