using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementApi.Services.Interfaces;
using UserManagementApi.ViewModels;

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
        var (success, message) = await _bookingService.CreateBooking(booking);
        if (success)
            return Ok(message);
        return BadRequest(message);
    }

    [Authorize(Roles = "Admin,User")]
    [HttpGet("ResourceHistory")]
    public async Task<IActionResult> GetResourceHistory([FromQuery] int? id = null)
    {
        var history = await _bookingService.GetBookingHistory(id);
        return Ok(history);
    }

    [Authorize(Roles = "Admin,User")]
    [HttpGet("ActiveBookings")]
    public async Task<IActionResult> GetActiveBookings([FromQuery] int? id = null)
    {
        var activeBookings = await _bookingService.GetActiveBookings(id);
        return Ok(activeBookings);
    }

    [Authorize(Roles = "Admin,User")]
    [HttpPost("ReleaseExpiredBookings")]
    public async Task<IActionResult> ReleaseExpiredBookings()
    {
        var (success, message) = await _bookingService.ReleaseExpiredBookings();
        if (success)
            return Ok(message);
        return BadRequest(message);
    }
}
