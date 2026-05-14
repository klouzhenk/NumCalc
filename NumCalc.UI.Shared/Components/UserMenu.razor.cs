using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using NumCalc.UI.Shared.Enums;
using NumCalc.UI.Shared.Resources;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.Components;

public partial class UserMenu : ComponentBase, IDisposable
{
    private const string ThemeKey = "is_dark_mode";
    
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IAuthStateService AuthStateService { get; set; } = null!;
    [Inject] private ITokenStorage TokenStorage { get; set; } = null!;
    [Inject] private IStorageService StorageService { get; set; } = null!;
    [Inject] private IJSRuntime JsRuntime { get; set; } = null!;
    [Inject] private ICultureService CultureService { get; set; } = null!;
    [Inject] private IStringLocalizer<Localization> Localizer { get; set; } = null!;
    [Inject] private ILogger<UserMenu> Logger { get; set; } = null!;
    [Inject] private IUiStateService UiStateService { get; set; } = null!;

    private BaseModal _languageModal = null!;
    private BaseModal _themeModal = null!;
    private bool _isOpen;
    private ThemeMode _themeMode;

    protected override void OnInitialized()
    {
        AuthStateService.OnAuthChanged += OnAuthStateChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _themeMode = await StorageService.LoadAsync<ThemeMode>(ThemeKey);
            await JsRuntime.InvokeVoidAsync("ThemeHelper.applyTheme", _themeMode.ToString());
            StateHasChanged();
        }
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
    
    private void OpenThemeModal()
    {
        Close();
        _themeModal.Show();
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

    private async Task ToggleTheme(ThemeMode themeMode)
    {
        if (_themeMode == themeMode) return;
        _themeMode = themeMode;
        await StorageService.SaveAsync(ThemeKey, themeMode);
        await JsRuntime.InvokeVoidAsync("ThemeHelper.applyTheme", themeMode.ToString());
    }

    public void Dispose()
    {
        AuthStateService.OnAuthChanged -= OnAuthStateChanged;
    }
}
