using Microsoft.AspNetCore.Mvc;
using UserManagementApi.Services.Interfaces;
using UserManagementApi.ViewModels;

namespace UserManagementApi.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] BookingViewModel booking)
    {
        var (success, message) = await _bookingService.CreateBooking(booking);
        if (success)
            return Ok(message);
        return BadRequest(message);
    }

    [HttpGet]
    public async Task<IActionResult> GetBookingHistory([FromQuery] int? userId = null)
    {
        var history = await _bookingService.GetBookingHistory(userId);
        return Ok(history);
    }

    [HttpGet]
    public async Task<IActionResult> GetActiveBookings([FromQuery] int? userId = null)
    {
        var activeBookings = await _bookingService.GetActiveBookings(userId);
        return Ok(activeBookings);
    }

    [HttpPost]
    public async Task<IActionResult> ReleaseExpiredBookings()
    {
        var (success, message) = await _bookingService.ReleaseExpiredBookings();
        if (success)
            return Ok(message);
        return BadRequest(message);
    }
}
