using NumCalc.UI.Shared.Enums;

namespace NumCalc.UI.Shared.Models.Message;

public class ErrorToastMessage(string message, string title = "Error") : ToastMessage(ToastType.Error, title, message)
{
}