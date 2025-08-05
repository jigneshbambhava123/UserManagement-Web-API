using Npgsql;
using Microsoft.Extensions;
using UserManagementApi.Services.Interfaces;
 
namespace UserManagementApi.Services.Implementations
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfiguration _configuration;
 
        public ConfigurationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
 
        public async Task<bool> IsMfaEnabledAsync()
        {
            await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();
 
            await using var cmd = new NpgsqlCommand("SELECT public.get_mfa_enabled()", conn);
            var result = await cmd.ExecuteScalarAsync();

            return result != null && (bool)result;
        }
    }
}