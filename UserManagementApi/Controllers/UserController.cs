using Microsoft.AspNetCore.Mvc;
using UserManagementApi.ViewModels;
using UserManagementApi.Services;
using UserManagementApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Data;
using System.IO;
using ExcelDataReader;

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

    [HttpPost("BulkInsertionUser")]
    public async Task<IActionResult> BulkInsertionUser(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is empty.");
        }

        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
    
        List<UserModel> users = new List<UserModel>();

        // var roleId;

        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);
            stream.Position = 0; 
    
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var result = reader.AsDataSet();
            var table = result.Tables[0];  

            for (int i = 1; i < table.Rows.Count; i++) 
            {
                var row = table.Rows[i];
                
                users.Add(new UserModel
                {
                    Firstname = table.Rows[i][0].ToString()?? string.Empty,
                    Lastname = table.Rows[i][1].ToString()?? string.Empty,
                    Email = table.Rows[i][2].ToString()?? string.Empty,
                    Password = table.Rows[i][3].ToString(),                    
                    RoleId = int.TryParse(table.Rows[i][4].ToString(), out var roleId)?roleId:0 ,
                    PhoneNumber = long.TryParse(table.Rows[i][4].ToString(), out var phonenumber)?phonenumber:0 ,
                    // PhoneNumber = Convert.ToInt64(table.Rows[i][5].ToString())?? string.Empty,
                    Dateofbirth = DateTime.TryParse(table.Rows[i][6].ToString(), out var dob) ? dob : DateTime.MinValue,
                });  
            }
        }
    
        var (successList, errorList) = await _userService.BulkInsertionUsers(users);
        return Ok(new { SuccessList = successList, ErrorList = errorList });
    }

    [Authorize(Roles = "Admin,User")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _userService.GetUserById(id);
        return Ok(user);
    }

    // [Authorize(Roles = "Admin")]
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

    // [Authorize(Roles = "Admin,User")]
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