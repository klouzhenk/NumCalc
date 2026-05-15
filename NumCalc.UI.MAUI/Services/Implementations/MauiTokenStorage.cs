using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.MAUI.Services.Implementations;

public class MauiTokenStorage : ITokenStorage
{
    private const string TokenKey = "auth_token";
    private const string UsernameKey = "auth_username";

    public async Task SaveAsync(string token, string username)
    {
        await SecureStorage.Default.SetAsync(TokenKey, token);
        await SecureStorage.Default.SetAsync(UsernameKey, username);
    }

    public async Task<(string? Token, string? Username)> LoadAsync()
    {
        try
        {
            var token = await SecureStorage.Default.GetAsync(TokenKey);
            var username = await SecureStorage.Default.GetAsync(UsernameKey);
            return (token, username);
        }
        catch
        {
            await ClearAsync();
            return (null, null);
        }
    }

    public Task ClearAsync()
    {
        SecureStorage.Default.Remove(TokenKey);
        SecureStorage.Default.Remove(UsernameKey);
        return Task.CompletedTask;
    }
}
