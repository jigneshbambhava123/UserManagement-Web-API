using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementApi.Services.Interfaces;
using UserManagementApi.ViewModels;
using UserManagementApi.Filters; 

namespace UserManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResourceController : ControllerBase
{
    private readonly IResourceService _resourceService;

    public ResourceController(IResourceService resourceService)
    {
        _resourceService = resourceService;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateResource([FromBody] ResourceViewModel resourceViewModel)
    {
        await _resourceService.CreateResource(resourceViewModel);
        return Ok("Resource created successfully.");
    }

    [Authorize(Roles = "Admin")]
    [HttpPut]
    public async Task<IActionResult> UpdateResource([FromBody] ResourceViewModel resourceViewModel)
    {
        await _resourceService.UpdateResource(resourceViewModel);
        return Ok("Resource updated successfully.");
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch]
    public async Task<IActionResult> UpdateField(int id, [FromBody] Dictionary<string, string> updateData)
    {
        await _resourceService.UpdateSingleField(id, updateData);
        return Ok("Resource field updated successfully."); 
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete]
    public async Task<IActionResult> DeleteResource(int id)
    {
        await _resourceService.DeleteResource(id);
        return Ok("Resource deleted successfully.");
    }

    [Authorize(Roles = "Admin,User")]
    [HttpGet("allresource")]
    public async Task<IActionResult> GetResources()
    {
        var resources = await _resourceService.GetAllResources();
        return Ok(resources);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetResourcesFiltered(
        [FromQuery] string? search,
        [FromQuery] string? sortColumn,
        [FromQuery] string? sortDirection,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var (resources, totalCount) = await _resourceService.GetAllResourcesFilteredAsync(
            search, sortColumn, sortDirection, pageNumber, pageSize);

        return Ok(new
        {
            data = resources,
            totalCount = totalCount
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetResourceById(int id)
    {
        var resource = await _resourceService.GetResourceById(id);
        return Ok(resource);
    }
}