using UserManagementApi.ViewModels;

namespace UserManagementApi.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<int> GetActiveUsersCount();
        Task<List<DailyResourceUsageViewModel>> GetDailyResourceUsage(int userId, int days = 30);
    }
}
