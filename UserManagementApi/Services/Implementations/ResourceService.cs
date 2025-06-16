using Npgsql;
using UserManagementApi.Services.Interfaces;
using UserManagementApi.ViewModels;

namespace UserManagementApi.Services.Implementations;

public class ResourceService : IResourceService
{
     private readonly IConfiguration _configuration;

    public ResourceService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<(bool Success, string Message)> CreateResource(Resource resource)
    {
        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("CALL public.create_resource(@p_name, @p_description, @p_quantity)", conn);
        cmd.Parameters.AddWithValue("p_name", resource.Name);
        cmd.Parameters.AddWithValue("p_description", resource.Description ?? string.Empty);
        cmd.Parameters.AddWithValue("p_quantity", resource.Quantity);

        await cmd.ExecuteNonQueryAsync();
        return (true, "Resource created successfully.");
    }

    public async Task<(bool Success, string Message)> UpdateResource(Resource resource)
    {
        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("CALL public.update_resource(@p_id, @p_name, @p_description, @p_quantity)", conn);
        cmd.Parameters.AddWithValue("p_id", resource.Id);
        cmd.Parameters.AddWithValue("p_name", resource.Name);
        cmd.Parameters.AddWithValue("p_description", resource.Description ?? string.Empty);
        cmd.Parameters.AddWithValue("p_quantity", resource.Quantity);

        await cmd.ExecuteNonQueryAsync();
        return (true, "Resource updated successfully.");
    }

    public async Task<(bool Success, string Message)> DeleteResource(int id)
    {
        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("CALL public.delete_resource(@p_id)", conn);
        cmd.Parameters.AddWithValue("p_id", id);

        await cmd.ExecuteNonQueryAsync();
        return (true, "Resource deleted successfully.");
    }

    public async Task<List<Resource>> GetAllResources()
    {
        var resources = new List<Resource>();

        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("SELECT * FROM public.get_all_resources()", conn);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            resources.Add(new Resource
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
                Quantity = reader.GetInt32(reader.GetOrdinal("quantity"))
            });
        }
        return resources;
    }
    
    public async Task<Resource?> GetResourceById(int id)
    {
        Resource? resource = null;

        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("SELECT * FROM public.get_resource_by_id(@p_id)", conn);
        cmd.Parameters.AddWithValue("p_id", id);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            resource = new Resource
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
                Quantity = reader.GetInt32(reader.GetOrdinal("quantity"))
            };
        }
        return resource;
    }
}

