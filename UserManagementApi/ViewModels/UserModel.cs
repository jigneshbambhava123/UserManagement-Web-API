using System.ComponentModel.DataAnnotations;
using UserManagementApi.Helper;
 
namespace UserManagementApi.ViewModels
{
    public class UserModel
    {
        public int Id { get; set; }
 
        [Required]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Firstname must be between 2 and 50 characters.")]
        public string Firstname { get; set; } = string.Empty;
 
        [Required]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Lastname must be between 2 and 50 characters.")]
        public string Lastname { get; set; } = string.Empty;
 
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;
 
        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
        ErrorMessage = "Password must be at least 8 characters, with one uppercase, one lowercase, one digit, and one special character.")]
        public string Password { get; set; } = string.Empty;
 
        public string PasswordHash { get; set; } = string.Empty;
 
        public string RoleName { get; set; } = string.Empty;
 
        [Required]
        [Range(1, 2, ErrorMessage = "RoleId must be either Admin or User.")]
        public int RoleId { get; set; }
 
        [Required]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
        public long PhoneNumber { get; set; }
 
        public bool IsActive { get; set; }
 
        [Required]
		[DataType(DataType.Date)]
        [MyDate(ErrorMessage = "Date of birth cannot be in the future.")]
        public DateTime Dateofbirth { get; set; }
    }
}