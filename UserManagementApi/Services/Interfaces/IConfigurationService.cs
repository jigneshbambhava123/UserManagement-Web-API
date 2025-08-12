namespace UserManagementApi.Services.Interfaces;

public interface IConfigurationService
{
    Task<bool> IsMfaEnabledAsync();
    Task UpdateMfaEnabledAsync(string key,bool isEnabled);
}
