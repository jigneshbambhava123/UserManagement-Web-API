using Microsoft.AspNetCore.Mvc;
using UserManagementApi.ViewModels;
using UserManagementApi.Services;
using UserManagementApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace UserManagementApi.Controllers;


[ApiController]
[Route("api/[controller]/[action]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] User user)
    {
        var (success, message) = await _userService.CreateUser(user);
        if (success)
            return Ok(message);
        return BadRequest(message);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _userService.GetUserById(id);
        if (user == null)
            return NotFound($"User with ID {id} not found.");
        return Ok(user);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateUser([FromBody] User user)
    {
        var (success, message) = await _userService.UpdateUser(user);
        if (success)
            return Ok(message);
        return BadRequest(message);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var result = await _userService.DeleteUser(id);
        if (!result)
            return NotFound($"User with ID {id} not found.");
        return Ok("User deleted (soft delete).");
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userService.GetUsers();
        return Ok(users);
    }

}
