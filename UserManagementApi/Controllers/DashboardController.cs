using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementApi.Services.Interfaces;
using UserManagementApi.ViewModels;

namespace UserManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [Authorize(Roles = "Admin,User")]
    [HttpGet("ActiveUsersCount")]
    public async Task<IActionResult> GetActiveUsersCount()
    {
        var count = await _dashboardService.GetActiveUsersCount();
        return Ok(new { activeUsersCount = count });
    }

    [Authorize(Roles = "Admin,User")]
    [HttpGet("DailyResourceUsage")]
    public async Task<IActionResult> GetDailyResourceUsage([FromQuery] int userId, [FromQuery] int days = 30)
    {
        var data = await _dashboardService.GetDailyResourceUsage(userId, days);
        return Ok(data);
    }
}