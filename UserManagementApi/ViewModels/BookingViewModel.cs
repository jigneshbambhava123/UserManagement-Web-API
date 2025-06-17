namespace UserManagementApi.ViewModels;

public class BookingViewModel
{
    public int Id { get; set; }
    public int ResourceId { get; set; }
    public int UserId { get; set; }
    public int Quantity { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string? ResourceName { get; set; }
}
