using NumCalc.UI.Shared.Enums;

namespace NumCalc.UI.Shared.Models.Message;

public class ToastMessage(ToastType toastType, string title, string message)
{
    public ToastType Type { get; } = toastType;
    
    public string Title { get; } = title;

    public string Message { get; } = message;
}