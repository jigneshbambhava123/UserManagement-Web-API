// In UserManagementApi.Services.Implementations/ResourceService.cs

using Npgsql;
using UserManagementApi.Services.Interfaces;
using UserManagementApi.ViewModels;
using UserManagementApi.Exceptions; // Add this using   directive

namespace UserManagementApi.Services.Implementations;

public class ResourceService : IResourceService
{
    private readonly IConfiguration _configuration;

    public ResourceService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task CreateResource(ResourceViewModel resourceViewModel)
    {
        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("CALL public.create_resource(@p_name, @p_description, @p_quantity)", conn);
        cmd.Parameters.AddWithValue("p_name", resourceViewModel.Name);
        cmd.Parameters.AddWithValue("p_description", resourceViewModel.Description ?? string.Empty);
        cmd.Parameters.AddWithValue("p_quantity", resourceViewModel.Quantity);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateResource(ResourceViewModel resourceViewModel)
    {
        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        var existsCmd = new NpgsqlCommand("SELECT 1 FROM public.resources WHERE id=@id AND \"isdeleted\"=false", conn);
        existsCmd.Parameters.AddWithValue("id", resourceViewModel.Id);
        if (await existsCmd.ExecuteScalarAsync() == null)
        {
            throw new NotFoundException($"Resource with ID {resourceViewModel.Id} not found.");
        }

        await using var cmd = new NpgsqlCommand("CALL public.update_resource(@p_id, @p_name, @p_description, @p_quantity)", conn);
        cmd.Parameters.AddWithValue("p_id", resourceViewModel.Id);
        cmd.Parameters.AddWithValue("p_name", resourceViewModel.Name);
        cmd.Parameters.AddWithValue("p_description", resourceViewModel.Description ?? string.Empty);
        cmd.Parameters.AddWithValue("p_quantity", resourceViewModel.Quantity);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateSingleField(int id, Dictionary<string, string> updateData)
    {
        if (updateData == null || updateData.Count == 0)
        {
            throw new ValidationException("No update data provided.");
        }
        if (updateData.Count > 1)
        {
            throw new ValidationException("Only one field can be updated at a time using this endpoint.");
        }

        var field = updateData.Keys.First();
        var value = updateData[field];

        await using var connCheck = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await connCheck.OpenAsync();
        var existsCmd = new NpgsqlCommand("SELECT 1 FROM public.resources WHERE id=@id AND \"isdeleted\"=false", connCheck);
        existsCmd.Parameters.AddWithValue("id", id);
        if (await existsCmd.ExecuteScalarAsync() == null)
        {
            throw new NotFoundException($"Resource with ID {id} not found.");
        }


        string procedureName = field.ToLower() switch
        {
            "name" => "update_resource_name",
            "description" => "update_resource_description",
            "quantity" => "update_resource_quantity",
            _ => null
        };

        if (procedureName == null)
        {
            throw new ValidationException($"Invalid field name: {field}. Allowed fields are 'name', 'description', 'quantity'.");
        }

        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand($"CALL public.{procedureName}(@p_id, @p_value)", conn);
        cmd.Parameters.AddWithValue("p_id", id);

        if (field.ToLower() == "quantity")
        {
            if (int.TryParse(value, out int quantity))
            {
                cmd.Parameters.AddWithValue("p_value", quantity);
            }
            else
            {
                throw new ValidationException("Quantity must be a valid integer.");
            }
        }
        else
        {
            cmd.Parameters.AddWithValue("p_value", value);
        }

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteResource(int id)
    {
        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        var existsCmd = new NpgsqlCommand("SELECT 1 FROM public.resources WHERE id=@id AND \"isdeleted\"=false", conn);
        existsCmd.Parameters.AddWithValue("id", id);
        if (await existsCmd.ExecuteScalarAsync() == null)
        {
            throw new NotFoundException($"Resource with ID {id} not found for deletion.");
        }

        await using var cmd = new NpgsqlCommand("CALL public.delete_resource(@p_id)", conn);
        cmd.Parameters.AddWithValue("p_id", id);

        await cmd.ExecuteNonQueryAsync();
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

    public async Task<ResourceViewModel> GetResourceById(int id)
    {
        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("SELECT * FROM public.get_resource_by_id(@p_id)", conn);
        cmd.Parameters.AddWithValue("p_id", id);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new ResourceViewModel
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
                Quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
                UsedQuantity = reader.GetInt32(reader.GetOrdinal("usedquantity"))
            };
        }
        throw new NotFoundException($"Resource with ID {id} not found.");
    }
}