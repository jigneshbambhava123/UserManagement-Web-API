using System.ComponentModel.DataAnnotations;

namespace UserManagementApi.ViewModels;

public class ResourceViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 characters.")]
    [RegularExpression(@"^(?!.*[\x00-\x1F\x7F])[a-zA-Z0-9_]+(?: [a-zA-Z0-9_]+)*$", ErrorMessage = "Name should only contain letters, numbers, underscores, and spaces between words. No leading/trailing spaces allowed.")]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    [Required(ErrorMessage = "Quantity is required.")]
    [Range(0, int.MaxValue, ErrorMessage = "Please enter a valid quantity of zero or more.")]
    public int Quantity { get; set; }

    public int? UsedQuantity { get; set; }
}

    