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

            var authResponse = new AuthResponse { Token = token, Username = username };
            AuthStateService.SetAuth(authResponse);
            StateHasChanged();
        }        
    }
    
    private void OnGlobalMouseDown()
    {
        UiStateService.RequestCloseDropdown();
    }
}