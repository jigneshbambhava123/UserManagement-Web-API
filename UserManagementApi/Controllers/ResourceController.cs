using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementApi.Services.Interfaces;
using UserManagementApi.ViewModels;

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
        var (success, message) = await _resourceService.CreateResource(resourceViewModel);
        if (success)
            return Ok(message);
        return BadRequest(message);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut]
    public async Task<IActionResult> UpdateResource([FromBody] ResourceViewModel resourceViewModel)
    {
        var (success, message) = await _resourceService.UpdateResource(resourceViewModel);
        if (success)
            return Ok(message);
        return BadRequest(message);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete]
    public async Task<IActionResult> DeleteResource(int id)
    {
        var (success, message) = await _resourceService.DeleteResource(id);
        if (success)
            return Ok(message);
        return BadRequest(message);
    }

    [Authorize(Roles = "Admin,User")]
    [HttpGet]
    public async Task<IActionResult> GetResources()
    {
        var resources = await _resourceService.GetAllResources();
        return Ok(resources);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetResourceById(int id)
    {
        var resource = await _resourceService.GetResourceById(id);
        if (resource == null)
            return NotFound("Resource not found.");
        return Ok(resource);
    }
}