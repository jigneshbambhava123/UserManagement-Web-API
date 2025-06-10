using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace UserManagementApi.Services;

public class TokenService: ITokenService
{

    public TokenService()
    {
    }

    public async Task<string> GenerateToken(string username, string email, string role, bool RememberMe, string configKey, string issuer, string audience, int userId)
    {
        var claims = new List<Claim>
        {
            new (ClaimTypes.Name, username),
            new (ClaimTypes.Email, email),
            new (ClaimTypes.Role, role) ,
            new (ClaimTypes.NameIdentifier, userId.ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: RememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddHours(24),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}
