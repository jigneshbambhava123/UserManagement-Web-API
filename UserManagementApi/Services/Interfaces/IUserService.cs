using UserManagementApi.ViewModels;

namespace UserManagementApi.Services.Interfaces;

public interface IUserService
{
    Task<(bool Success, string Message)> CreateUser(User user);
    Task<(bool Success, string Message)> UpdateUser(User user);
    Task<bool> DeleteUser(int id);
    Task<IEnumerable<User>> GetUsers();
    Task<bool> EmailExists(string email, int? excludeUserId = null);
    Task<User?> GetUserById(int id);

}
