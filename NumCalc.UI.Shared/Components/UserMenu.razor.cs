using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using NumCalc.UI.Shared.Resources;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.Components;

public partial class UserMenu : ComponentBase, IDisposable
{
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IAuthStateService AuthStateService { get; set; } = null!;
    [Inject] private ITokenStorage TokenStorage { get; set; } = null!;
    [Inject] private ICultureService CultureService { get; set; } = null!;
    [Inject] private IStringLocalizer<Localization> Localizer { get; set; } = null!;
    [Inject] private ILogger<UserMenu> Logger { get; set; } = null!;
    [Inject] private IUiStateService UiStateService { get; set; } = null!;

    private BaseModal _languageModal = null!;
    private bool _isOpen;

    protected override void OnInitialized()
    {
        AuthStateService.OnAuthChanged += OnAuthStateChanged;
    }

    private void OnAuthStateChanged() => StateHasChanged();

    private void Toggle() => _isOpen = !_isOpen;
    private void Close() => _isOpen = false;

    private void GoToAccountSettings()
    {
        Close();
        NavigationManager.NavigateTo("/account-settings");
    }

    private void GoToDashboard()
    {
        Close();
        NavigationManager.NavigateTo("/dashboard");
    }

    private void GoToLogin()
    {
        Close();
        NavigationManager.NavigateTo("/login");
    }

    private async Task Logout()
    {
        Close();
        await TokenStorage.ClearAsync();
        AuthStateService.ClearAuth();
        NavigationManager.NavigateTo("/", true);
    }

    private void OpenLanguageModal()
    {
        Close();
        _languageModal.Show();
    }

    private async Task SelectCulture(string culture)
    {
        try
        {
            await _languageModal.Close();
            await CultureService.SetCulture(culture);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "{msg}", ex.Message);
            UiStateService.ShowError("Unable to set culture");
        }
    }

    public void Dispose()
    {
        AuthStateService.OnAuthChanged -= OnAuthStateChanged;
    }
}
