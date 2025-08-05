namespace UserManagementApi.Services.Interfaces;

public interface IOtpService
{
    Task SendOtpEmailAsync(string email);
    Task<bool> VerifyOtpAsync(string email, string otpCode);
}
