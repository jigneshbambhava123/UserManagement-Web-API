using UserManagementApi.ViewModels;

namespace UserManagementApi.Services.Interfaces;

public interface IUserService
{
    Task CreateUser(UserViewModel userViewModel);
    Task<(List<UserModel> successList, List<object> errorList)> BulkInsertionUsers(List<UserModel> users);
    Task UpdateUser(UserViewModel userViewModel);
    Task DeleteUser(int id);
    Task<(List<UserViewModel>, int)> GetUsers(string? search, string? sortColumn, string? sortDirection, int pageNumber, int pageSize);
    Task<UserViewModel?> GetUserById(int id);
    Task<bool> EmailExists(string email, int? excludeUserId = null);
    Task UpdateUserLanguage(int userId, string language);
    Task<string> GetUserLanguage(int userId);
}
