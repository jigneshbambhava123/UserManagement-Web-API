using Microsoft.AspNetCore.Mvc;
using UserManagementApi.ViewModels;
using UserManagementApi.Services;
using UserManagementApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

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
        await _userService.CreateUser(userViewModel);
        return Ok("User created successfully.");
    }

    [Authorize(Roles = "Admin,User")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _userService.GetUserById(id);
        return Ok(user);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut]
    public async Task<IActionResult> UpdateUser([FromBody] UserViewModel userViewModel)
    {
        await _userService.UpdateUser(userViewModel);
        return Ok("User updated successfully.");
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete] 
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _userService.DeleteUser(id);
        return Ok("User deleted successfully.");
    }

    [Authorize(Roles = "Admin,User")]
    [HttpGet]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? search,
        [FromQuery] string? sortColumn,
        [FromQuery] string? sortDirection,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var validSortColumns = new List<string> { "firstname", "lastname", "email", "roleid", "phonenumber" };
        sortColumn = validSortColumns.Contains(sortColumn?.ToLower() ?? "") ? sortColumn : "firstname";
        sortDirection = sortDirection?.ToLower() == "desc" ? "desc" : "asc";

        var (users, totalCount) = await _userService.GetUsers(search, sortColumn, sortDirection, pageNumber, pageSize);

        return Ok(new
        {
            data = users,
            totalCount = totalCount
        });
    }

}