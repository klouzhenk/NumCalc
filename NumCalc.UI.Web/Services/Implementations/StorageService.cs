using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using NumCalc.UI.Shared.Services.Interfaces;

namespace WebUI.Services.Implementations;

public class StorageService(ProtectedLocalStorage storage) : IStorageService
{
    public async Task SaveAsync<T>(string key, T value) where T : notnull
    {
        await storage.SetAsync(key, value);
    }

    public async Task<T?> LoadAsync<T>(string key)
    {
        try
        {
            var result = await storage.GetAsync<T>(key);
            return result.Success ? result.Value : default;
        }
        catch
        {
            return default;
        }
    }

    public async Task ClearAsync(string key)
    {
        await storage.DeleteAsync(key);
    }
}