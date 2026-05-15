using System.Security.Cryptography;
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
        try
        {
            var token = await storage.GetAsync<string>(TokenKey);
            var username = await storage.GetAsync<string>(UsernameKey);
            return (token.Success ? token.Value : null, username.Success ? username.Value : null);
        }
        catch (CryptographicException)
        {
            // Data Protection key ring changed (e.g. container restart with ephemeral keys).
            // Treat as logged-out and wipe the unreadable blobs so we don't retry every render.
            await ClearAsync();
            return (null, null);
        }
    }

    public async Task ClearAsync()
    {
        await storage.DeleteAsync(TokenKey);
        await storage.DeleteAsync(UsernameKey);
    }
}