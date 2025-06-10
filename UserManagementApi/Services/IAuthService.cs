using UserManagementApi.ViewModels;

namespace UserManagementApi.Services;

public interface IAuthService
{
    Task<User?> AuthenticateUserAsync(string email, string password);
}
