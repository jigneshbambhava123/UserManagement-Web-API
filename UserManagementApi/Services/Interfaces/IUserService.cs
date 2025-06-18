using UserManagementApi.ViewModels;

namespace UserManagementApi.Services.Interfaces;

public interface IUserService
{
    Task<(bool Success, string Message)> CreateUser(UserViewModel userViewModel);
    Task<(bool Success, string Message)> UpdateUser(UserViewModel userViewModel);
    Task<(bool Success, string Message)> DeleteUser(int id);
    Task<IEnumerable<UserViewModel>> GetUsers();
    Task<bool> EmailExists(string email, int? excludeUserId = null);
    Task<UserViewModel?> GetUserById(int id);
}
