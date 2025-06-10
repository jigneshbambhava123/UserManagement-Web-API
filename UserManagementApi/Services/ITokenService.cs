namespace UserManagementApi.Services;

public interface ITokenService
{
    Task<string> GenerateToken(string username, string email, string role, bool RememberMe, string configKey, string issuer, string audience, int userId);

}
