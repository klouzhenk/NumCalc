using Microsoft.AspNetCore.Components;
using NumCalc.UI.Shared.Enums;
using NumCalc.UI.Shared.Models.Message;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.Components;

public partial class ToastContainer : ComponentBase, IDisposable
{
    [Inject] protected IUiStateService UiStateService { get; set; } = null!;

    private List<ToastMessage> _toasts = [];

    protected override void OnInitialized()
    {
        UiStateService.OnShowToast += HandleToast;
    }

    private void HandleToast(ToastMessage toast)
    {
        InvokeAsync(async () =>
        {
            _toasts.Add(toast);
            StateHasChanged();

            await Task.Delay(4000);
            RemoveToast(toast);
        });
    }

    private void RemoveToast(ToastMessage toast)
    {
        if (!_toasts.Contains(toast)) return;
        
        _toasts.Remove(toast);
        InvokeAsync(StateHasChanged);
    }

    private static string GetToastClass(ToastMessage toast) => toast.Type switch
    {
        ToastType.Success => "toast-success",
        ToastType.Error => "toast-error",
        ToastType.Warning => "toast-warning",
        _ => "toast-info"
    };

    private static string GetIcon(ToastMessage toast) => toast.Type switch
    {
        ToastType.Success => "✅",
        ToastType.Error => "❌",
        ToastType.Warning => "⚠️",
        _ => "ℹ️"
    };

    public void Dispose()
    {
        UiStateService.OnShowToast -= HandleToast;
    }
}