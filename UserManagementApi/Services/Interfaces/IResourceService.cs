using UserManagementApi.ViewModels;

namespace UserManagementApi.Services.Interfaces;

public interface IResourceService
{
        Task CreateResource(ResourceViewModel resourceViewModel);
        Task UpdateResource(ResourceViewModel resourceViewModel);
        Task UpdateSingleField(int id, Dictionary<string, string> updateData);
        Task DeleteResource(int id);
        Task<List<ResourceViewModel>> GetAllResources();
        Task<(List<ResourceViewModel>, int)> GetAllResourcesFilteredAsync(string? search,string? sortColumn,string? sortDirection,int pageNumber,int pageSize);
        Task<ResourceViewModel> GetResourceById(int id);
}
