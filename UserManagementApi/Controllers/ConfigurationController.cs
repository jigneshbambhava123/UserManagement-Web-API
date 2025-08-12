using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using UserManagementApi.Services.Interfaces;
 
namespace UserManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        private readonly IConfigurationService _configurationService;
 
        public ConfigurationController(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }
 
        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> UpdateMfaEnabled([FromQuery] string key,[FromQuery] bool isEnabled)
        {
            await _configurationService.UpdateMfaEnabledAsync(key, isEnabled);
            return Ok(new { message = "updated successfully!.", enableMfa = isEnabled });
        }
    }
}
 