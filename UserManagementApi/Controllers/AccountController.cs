using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using UserManagementApi.Helper;
using UserManagementApi.Services.Interfaces;
using UserManagementApi.ViewModels;

namespace UserManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAuthService _authService; 
    private readonly IConfiguration _configuration;
    private readonly ITokenService _tokenService;
    private readonly IUserService _userService;

    public AccountController(IAuthService authService, IConfiguration configuration, ITokenService tokenService,IUserService userService)
    {
        _authService = authService;
        _configuration = configuration;
        _tokenService = tokenService;
        _userService = userService;
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginViewModel loginModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _authService.AuthenticateUserAsync(loginModel.Email, loginModel.Password);

        if (user == null || !PasswordHasher.VerifyPassword(loginModel.Password, user.PasswordHash))
            return Unauthorized(new { message = "The email or password you entered is incorrect. Please try again." });

        var jwtKey = _configuration["JwtSettings:Key"];
        if (string.IsNullOrEmpty(jwtKey))
            return StatusCode(500, "JWT Key not configured");

        var token = await _tokenService.GenerateToken(
            user.Firstname,
            user.Email,
            user.RoleName,
            loginModel.RememberMe,
            jwtKey,
            "localhost",
            "localhost",
            user.Id
        );
        Console.WriteLine("token"+token);

        return Ok(new { token });
    }

    [HttpPost("ForgotPassword")]
    public async Task<IActionResult> ForgotPassword([FromQuery] string email, [FromQuery] string baseUrl)
    {
        var result = await _authService.ForgotPasswordAsync(email, baseUrl);
        return Ok(new { token = result.Token, userId = result.UserId, message = result.Message });
    }

    [HttpGet]   
    public async Task<IActionResult> ValidateResetToken(int userId, string token)
    {
        var isValid = await _authService.ValidateResetTokenAsync(userId, token);

        if (!isValid)
            return BadRequest("Token is invalid or expired. Please obtain a new token.");

        return Ok("Valid token");
    }

    [HttpPost("ResetPassword")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
    {
        var result = await _authService.ResetPasswordAsync(model.UserId, model.Token, model.NewPassword);
        return Ok(result);
    }


    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] UserViewModel userViewModel)
    {
        await _userService.CreateUser(userViewModel);
        return Ok("User Register successfully.");
    }
}
