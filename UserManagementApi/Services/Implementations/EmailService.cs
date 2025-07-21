using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using UserManagementApi.Services.Interfaces;

namespace UserManagementApi.Services.Implementations;

public class EmailService: IEmailService
{
    public async Task SendPasswordResetEmail(string email, string resetLink)
    {
        try
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("jigneshbambhava111@gmail.com", "wvyu jzjk xegr jzak"),
                EnableSsl = true,
            };

            string emailBody = $@"
                <html>
                    <body>
                        <p>Hello,</p>
                        <p>You requested a password reset. Click the link below:</p>
                        <p><a href='{resetLink}'>{resetLink}</a></p>
                        <p>This link will expire in 1 hour.</p>
                    </body>
                </html>";

            var mailMessage = new MailMessage
            {
                From = new MailAddress("jigneshbambhava111@gmail.com", "User Management"),
                Subject = "Reset Your Password",
                Body = emailBody,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(email);
            await smtpClient.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending reset password email: {ex.Message}");
        }
    }

    public async Task SendAccountDetailsEmail(string email, string username, string password)
    {
        try
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("jigneshbambhava111@gmail.com", "wvyu jzjk xegr jzak"),
                EnableSsl = true,
            };

            string emailBody = $@"
                <html>
                    <body>
                        <p>Hello,</p>
                        <p>Your account has been created. Here are your credentials:</p>
                        <p>Email: {email}</p>
                        <p>Username: {username}</p>
                        <p>Password: {password}</p>
                    </body>
                </html>";

            var mailMessage = new MailMessage
            {
                From = new MailAddress("jigneshbambhava111@gmail.com", "UserManagement"),
                Subject = "Your Account Details",
                Body = emailBody,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(email);
            await smtpClient.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending account details email: {ex.Message}");
        }
    }
}
    