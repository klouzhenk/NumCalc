using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using NumCalc.Shared.Common;
using NumCalc.UI.Shared.Exceptions;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.Export;
using NumCalc.UI.Shared.Models.User;
using NumCalc.UI.Shared.Models.User.Enums;
using NumCalc.UI.Shared.Resources;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.Pages;

public abstract class BasePage<TPageType> : ComponentBase, IDisposable
{
    [Inject] protected IUiStateService UiService { get; set; } = null!;
    [Inject] protected IStringLocalizer<Localization> Localizer { get; set; } = null!;
    [Inject] protected IJSRuntime JsRuntime { get; set; } = null!;
    [Inject] protected NavigationManager Navigation { get; set; } = null!;
    [Inject] protected ILogger<TPageType> Logger { get; set; } = null!;
    [Inject] protected IAuthStateService AuthStateService { get; set; } = null!;

    protected bool IsAuthenticated => AuthStateService.IsAuthenticated;

    protected override void OnInitialized()
    {
        AuthStateService.OnAuthChanged += OnAuthStateChanged;
    }

    protected virtual void OnAuthStateChanged() => InvokeAsync(StateHasChanged);
    
    protected async Task<T?> SafeExecuteAsync<T>(Func<Task<T>> action, T? defaultValue = default)
    {
        UiService.ShowLoader();
        try
        {
            return await action();
        }
        catch (ApiException ex)
        {
            Logger.LogError(ex, "API error: {Message}", ex.Message);
            UiService.ShowError(ex.Message);
            return defaultValue;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            UiService.ShowError("SomethingWentWrong");
            return defaultValue;
        }
        finally
        {
            UiService.HideLoader();
        }
    }

    protected async Task SafeExecuteAsync(Func<Task> action)
    {
        UiService.ShowLoader();
        try
        {
            await action();
        }
        catch (ApiException ex)
        {
            Logger.LogError(ex, "API error: {Message}", ex.Message);
            UiService.ShowError(ex.Message);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            UiService.ShowError("SomethingWentWrong");
        }
        finally
        {
            UiService.HideLoader();
        }
    }

    public virtual void Dispose()
    {
        AuthStateService.OnAuthChanged -= OnAuthStateChanged;
    }
}