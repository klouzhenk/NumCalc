namespace NumCalc.UI.Shared.Services.Interfaces;

public interface IStorageService
{
    Task SaveAsync<T>(string key, T value) where T : notnull;
    Task<T?> LoadAsync<T>(string key);
    Task ClearAsync(string key);
}