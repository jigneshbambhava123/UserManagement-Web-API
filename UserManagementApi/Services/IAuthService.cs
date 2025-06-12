using UserManagementApi.ViewModels;

namespace UserManagementApi.Services;

public interface IAuthService
{
    Task<User?> AuthenticateUserAsync(string email, string password);
    Task<(bool Success, string Token, int UserId, string Message)> ForgotPasswordAsync(string email, string baseUrl);
    Task<bool> ValidateResetTokenAsync(int userId, string token);
    Task<string> ResetPasswordAsync(int userId, string token, string newPassword);
}
