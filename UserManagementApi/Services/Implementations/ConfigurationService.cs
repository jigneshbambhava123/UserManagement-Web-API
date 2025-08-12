using Npgsql;
using Microsoft.Extensions;
using UserManagementApi.Services.Interfaces;
using UserManagementApi.Exceptions;

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
        public async Task UpdateMfaEnabledAsync(string key,bool isEnabled)
        {
            await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

             var existsCmd = new NpgsqlCommand("SELECT 1 FROM public.configuration WHERE key = @key", conn);
            existsCmd.Parameters.AddWithValue("key", key);

            var exists = await existsCmd.ExecuteScalarAsync() != null;
            if (!exists)
            {
                throw new NotFoundException($"Configuration key '{key}' not found.");
            }
 
            await using var cmd = new NpgsqlCommand("CALL public.update_config_value(@key, @value)", conn);
            cmd.Parameters.AddWithValue("@key", key);
            cmd.Parameters.AddWithValue("@value", isEnabled);
 
            await cmd.ExecuteNonQueryAsync();
        }
    }
}