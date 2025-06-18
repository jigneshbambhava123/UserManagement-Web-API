using UserManagementApi.ViewModels;

namespace UserManagementApi.Services.Interfaces;

public interface IResourceService
{
    Task<(bool Success, string Message)> CreateResource(ResourceViewModel resourceViewModel);
    Task<(bool Success, string Message)> UpdateResource(ResourceViewModel resourceViewModel);
    Task<(bool Success, string Message)> DeleteResource(int id);
    Task<List<ResourceViewModel>> GetAllResources();
    Task<ResourceViewModel?> GetResourceById(int id);
}
