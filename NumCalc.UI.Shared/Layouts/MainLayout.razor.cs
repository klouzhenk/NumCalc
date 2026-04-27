using Microsoft.AspNetCore.Components;
using NumCalc.UI.Shared.Models.User;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.Layouts;

public partial class MainLayout : LayoutComponentBase
{
    [Inject] private ITokenStorage TokenStorage { get; set; } = null!;
    [Inject] private IAuthStateService AuthStateService { get; set; } = null!;
    [Inject] private IUiStateService UiStateService { get; set; } = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var (token, username) = await TokenStorage.LoadAsync();
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(username)) return;

            if (IsTokenExpired(token))
            {
                await TokenStorage.ClearAsync();
                return;
            }

            var authResponse = new AuthResponse { Token = token, Username = username };
            AuthStateService.SetAuth(authResponse);
            StateHasChanged();
        }
    }

    private static bool IsTokenExpired(string token)
    {
        try
        {
            var payload = token.Split('.')[1];
            var padded = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(padded));
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("exp", out var exp))
                return DateTimeOffset.FromUnixTimeSeconds(exp.GetInt64()) <= DateTimeOffset.UtcNow;
            return false;
        }
        catch { return true; }
    }
    
    private void OnGlobalMouseDown()
    {
        UiStateService.RequestCloseDropdown();
    }
}