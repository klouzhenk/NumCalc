using System.Text.Json;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.MAUI.Services.Implementations;

public class MauiStorageService : IStorageService
{
    public Task SaveAsync<T>(string key, T value) where T : notnull
    {
        var json = JsonSerializer.Serialize(value);
        Preferences.Default.Set(key, json);
        return Task.CompletedTask;
    }

    public Task<T?> LoadAsync<T>(string key)
    {
        try
        {
            var json = Preferences.Default.Get<string?>(key, null);
            return string.IsNullOrEmpty(json)
                ? Task.FromResult<T?>(default)
                : Task.FromResult(JsonSerializer.Deserialize<T>(json));
        }
        catch
        {
            return Task.FromResult<T?>(default);
        }
    }

    public Task ClearAsync(string key)
    {
        Preferences.Default.Remove(key);
        return Task.CompletedTask;
    }
}
