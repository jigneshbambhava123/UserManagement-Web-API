namespace UserManagementApi.ViewModels;

public class OtpVerifyViewModel
{
    public string Email { get; set; }
    public string Otp { get; set; }
    public bool RememberMe {get; set; }
}
