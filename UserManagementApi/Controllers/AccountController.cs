using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using UserManagementApi.Helper;
using UserManagementApi.Services;
using UserManagementApi.ViewModels;

namespace UserManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAuthService _authService; // your user auth service
    private readonly IConfiguration _configuration;
    private readonly ITokenService _tokenService;

    public AccountController(IAuthService authService, IConfiguration configuration, ITokenService tokenService)
    {
        _authService = authService;
        _configuration = configuration;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginViewModel loginModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _authService.AuthenticateUserAsync(loginModel.Email, loginModel.Password);

        if (user == null || !PasswordHasher.VerifyPassword(loginModel.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid email or password" });

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

        return Ok(new { token });
    }
}
