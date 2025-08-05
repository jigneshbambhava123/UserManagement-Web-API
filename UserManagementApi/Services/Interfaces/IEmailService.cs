namespace UserManagementApi.Services.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetEmail(string email, string resetLink);
    Task SendAccountDetailsEmail(string email, string username, string password);
    Task SendOtpEmail(string email, string otpCode);
}
