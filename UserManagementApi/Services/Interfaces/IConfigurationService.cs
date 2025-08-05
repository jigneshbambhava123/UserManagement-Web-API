namespace UserManagementApi.Services.Interfaces;

public interface IConfigurationService
{
    Task<bool> IsMfaEnabledAsync();
}
