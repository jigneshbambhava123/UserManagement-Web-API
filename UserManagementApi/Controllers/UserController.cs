using Microsoft.AspNetCore.Mvc;
using UserManagementApi.ViewModels;
using UserManagementApi.Services;
using UserManagementApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace UserManagementApi.Controllers;


[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] UserViewModel userViewModel)
    {
        var (success, message) = await _userService.CreateUser(userViewModel);
        if (success)
            return Ok(message);
        return BadRequest(message);
    }

    [Authorize(Roles = "Admin,User")]
    [HttpGet("{id}")] 
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _userService.GetUserById(id);
        if (user == null)
            return NotFound($"User with ID {id} not found.");
        return Ok(user);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut]
    public async Task<IActionResult> UpdateUser([FromBody] UserViewModel userViewModel)
    {
        var (success, message) = await _userService.UpdateUser(userViewModel);
        if (success)
            return Ok(message);
        return BadRequest(message);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var (success, message) = await _userService.DeleteUser(id);

        if(success)
            return Ok(message);
        return BadRequest(message);
    }

    [Authorize(Roles = "Admin,User")]
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userService.GetUsers();
        return Ok(users);
    }

}
