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

    public async Task<(bool Success, string Message)> CreateResource(ResourceViewModel resourceViewModel)
    {
        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("CALL public.create_resource(@p_name, @p_description, @p_quantity)", conn);
        cmd.Parameters.AddWithValue("p_name", resourceViewModel.Name);
        cmd.Parameters.AddWithValue("p_description", resourceViewModel.Description ?? string.Empty);
        cmd.Parameters.AddWithValue("p_quantity", resourceViewModel.Quantity);

        await cmd.ExecuteNonQueryAsync();
        return (true, "Resource created successfully.");
    }

    public async Task<(bool Success, string Message)> UpdateResource(ResourceViewModel resourceViewModel)
    {
        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("CALL public.update_resource(@p_id, @p_name, @p_description, @p_quantity)", conn);
        cmd.Parameters.AddWithValue("p_id", resourceViewModel.Id);
        cmd.Parameters.AddWithValue("p_name", resourceViewModel.Name);
        cmd.Parameters.AddWithValue("p_description", resourceViewModel.Description ?? string.Empty);
        cmd.Parameters.AddWithValue("p_quantity", resourceViewModel.Quantity);

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

    public async Task<List<ResourceViewModel>> GetAllResources()
    {
        var resources = new List<ResourceViewModel>();

        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("SELECT * FROM public.get_all_resources()", conn);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            resources.Add(new ResourceViewModel
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
                Quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
                UsedQuantity = reader.GetInt32(reader.GetOrdinal("usedquantity")) 
            });
        }
        return resources;
    }

    public async Task<ResourceViewModel?> GetResourceById(int id)
    {
        ResourceViewModel? resource = null;

        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("SELECT * FROM public.get_resource_by_id(@p_id)", conn);
        cmd.Parameters.AddWithValue("p_id", id);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            resource = new ResourceViewModel
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
                Quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
                UsedQuantity = reader.GetInt32(reader.GetOrdinal("usedquantity")) 
            };
        }
        return resource;
    }
}

