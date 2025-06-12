using System.ComponentModel.DataAnnotations;

namespace UserManagementApi.ViewModels;

public class ResetPasswordViewModel
{
     public int UserId { get; set; }
    public string Token { get; set; }

    [Required, DataType(DataType.Password)]
    public string NewPassword { get; set; }

    [Required, DataType(DataType.Password), Compare("NewPassword")]
    public string ConfirmPassword { get; set; }
}
