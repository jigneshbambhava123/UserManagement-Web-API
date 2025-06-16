using UserManagementApi.ViewModels;

namespace UserManagementApi.Services.Interfaces;

public interface IResourceService
{
    Task<(bool Success, string Message)> CreateResource(Resource resource);
    Task<(bool Success, string Message)> UpdateResource(Resource resource);
    Task<(bool Success, string Message)> DeleteResource(int id);
    Task<List<Resource>> GetAllResources();
    Task<Resource?> GetResourceById(int id);
}
