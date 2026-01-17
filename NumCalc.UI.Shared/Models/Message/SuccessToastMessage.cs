using NumCalc.UI.Shared.Enums;

namespace NumCalc.UI.Shared.Models.Message;

public class SuccessToastMessage(string message, string title = "Success") : ToastMessage(ToastType.Success, title, message)
{
}