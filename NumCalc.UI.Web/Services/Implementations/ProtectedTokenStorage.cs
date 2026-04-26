using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using NumCalc.UI.Shared.Services.Interfaces;

namespace WebUI.Services.Implementations;

public class ProtectedTokenStorage(ProtectedLocalStorage storage) : ITokenStorage
{
    private const string TokenKey = "auth_token";
    private const string UsernameKey = "auth_username";
    
    public async Task SaveAsync(string token, string username)
    {
        await storage.SetAsync(TokenKey, token);
        await storage.SetAsync(UsernameKey, username);
    }

    public async Task<(string? Token, string? Username)> LoadAsync()
    {
        var token = await storage.GetAsync<string>(TokenKey);
        var username = await storage.GetAsync<string>(UsernameKey);
        return (token.Success ? token.Value : null, username.Success ? username.Value : null);
    }

    public async Task ClearAsync()
    {
        await storage.DeleteAsync(TokenKey);
        await storage.DeleteAsync(UsernameKey);
    }
}