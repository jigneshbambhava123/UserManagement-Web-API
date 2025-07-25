using System.ComponentModel.DataAnnotations;

namespace UserManagementApi.Helper;

public class MyDateAttribute: ValidationAttribute
{
    public override bool IsValid(object value)
    {
        DateTime d = Convert.ToDateTime(value);
        return d <= DateTime.Now; 
    }
}
