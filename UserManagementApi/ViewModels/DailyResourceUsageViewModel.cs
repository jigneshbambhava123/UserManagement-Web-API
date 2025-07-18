namespace UserManagementApi.ViewModels;

public class DailyResourceUsageViewModel
{
    public DateTime BookingDate { get; set; }
    public long TotalActiveQuantity { get; set; }
    public long TotalUsedQuantity { get; set; }
}
