using NumCalc.UI.Shared.Models.Message;

namespace NumCalc.UI.Shared.Services.Interfaces;

public interface IUiStateService
{
    event Action<bool> OnLoaderChanged;
    event Action<ToastMessage> OnShowToast;
    event Action OnCloseDropdownRequested;

    void ShowLoader();
    void HideLoader();
    void ShowError(string message, string title = "Error");
    void ShowSuccess(string message, string title = "Success");
    void RequestCloseDropdown();
}