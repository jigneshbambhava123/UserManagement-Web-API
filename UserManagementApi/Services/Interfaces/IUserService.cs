using UserManagementApi.ViewModels;

namespace UserManagementApi.Services.Interfaces;

public interface IUserService
{
    Task CreateUser(UserViewModel userViewModel);
    Task UpdateUser(UserViewModel userViewModel);
    Task DeleteUser(int id);
    Task<IEnumerable<UserViewModel>> GetUsers();
    Task<UserViewModel?> GetUserById(int id);
    Task<bool> EmailExists(string email, int? excludeUserId = null);
}
