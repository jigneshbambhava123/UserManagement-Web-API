namespace UserManagementApi.ViewModels;

public class ResourceViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public int? UsedQuantity { get; set; }
}
