namespace UserManagementApi.ViewModels;

public class Resource
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int Quantity { get; set; }
}
