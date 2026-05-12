using NumCalc.UI.Shared.Enums;
using NumCalc.UI.Shared.Models.Message;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.Services.Implementations;

public class UiStateService : IUiStateService
{
    private int _busyCount = 0;
    
    public event Action<bool>? OnLoaderChanged;
    public event Action<ToastMessage>? OnShowToast;
    public event Action? OnCloseDropdownRequested;
    public event Action? OnNavMenuChanged;
    public event Action<NavigationItem>? OnTopicInfoRequested;

    public bool IsNavMenuOpen { get; private set; }

    public void ToggleNavMenu()
    {
        IsNavMenuOpen = !IsNavMenuOpen;
        OnNavMenuChanged?.Invoke();
    }

    public void CloseNavMenu()
    {
        if (!IsNavMenuOpen) return;
        IsNavMenuOpen = false;
        OnNavMenuChanged?.Invoke();
    }

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

    public void RequestCloseDropdown() => 
        OnCloseDropdownRequested?.Invoke();
    
    public void RequestTopicInfo(NavigationItem item) =>
        OnTopicInfoRequested?.Invoke(item);
}