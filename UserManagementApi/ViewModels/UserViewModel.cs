namespace UserManagementApi.ViewModels;

public class UserViewModel
{
    public int Id { get; set; }
    public string Firstname { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;  
    public string RoleName { get; set; } = string.Empty;    
    public int RoleId { get; set; }
    public long PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public DateTime Dateofbirth { get; set; }  
}
