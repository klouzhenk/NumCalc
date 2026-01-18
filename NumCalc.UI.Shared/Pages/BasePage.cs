using Microsoft.AspNetCore.Components;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.Pages;

public abstract class BasePage : ComponentBase
{
    [Inject] protected IUiStateService UiService { get; set; } = null!;
    [Inject] protected NavigationManager Navigation { get; set; } = null!;
    
    // TODO : implement localization
    
    protected async Task<T?> SafeExecuteAsync<T>(Func<Task<T>> action, T? defaultValue = default)
    {
        UiService.ShowLoader();
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            UiService.ShowError(ex.Message);
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
        catch (Exception ex)
        {
            UiService.ShowError(ex.Message);
        }
        finally
        {
            UiService.HideLoader();
        }
    }
}