using UserManagementApi.ViewModels;

namespace UserManagementApi.ViewModels;

public class ErrorUserViewModel
{
    public UserViewModel User { get; set; }
    public string Reason { get; set; }
}
