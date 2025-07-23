namespace UserManagementApi.Helper;
using UserManagementApi.ViewModels;
using System.ComponentModel.DataAnnotations;

public static class ModelValidator
{
    public static List<ValidationResult> validate(UserModel user)
    {
        var context = new ValidationContext(user, serviceProvider: null, items: null);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(user, context, results, validateAllProperties: true);
        return results;
    }
}
