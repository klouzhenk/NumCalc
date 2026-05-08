namespace NumCalc.UI.Shared.Services.Interfaces;

public interface ITokenStorage
{
    Task SaveAsync(string token, string username);
    Task<(string? Token, string? Username)> LoadAsync();
    Task ClearAsync();
}