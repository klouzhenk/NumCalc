using NumCalc.UI.Shared.Models.Message;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.Services.Implementations;

public class UiStateService : IUiStateService
{
    private int _busyCount = 0;
    
    public event Action<bool>? OnLoaderChanged;
    public event Action<ToastMessage>? OnShowToast;
    public event Action? OnCloseDropdownRequested;

    public void ShowLoader()
    {
        _busyCount++;
        UpdateLoaderState();
    }

    public void HideLoader()
    {
        if (_busyCount > 0)
        {
            _busyCount--;
        }
        UpdateLoaderState();
    }
    
    private void UpdateLoaderState()
    {
        var shouldShow = _busyCount > 0;
        OnLoaderChanged?.Invoke(shouldShow);
    }

    public void ShowError(string message, string title = "Error") 
        => OnShowToast?.Invoke(new ErrorToastMessage(message, title));

    public void ShowSuccess(string message, string title = "Success")
        => OnShowToast?.Invoke(new SuccessToastMessage(message, title));

    public void RequestCloseDropdown() => OnCloseDropdownRequested?.Invoke();
}