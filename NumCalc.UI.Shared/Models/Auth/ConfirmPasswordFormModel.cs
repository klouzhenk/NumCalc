using System.ComponentModel.DataAnnotations;
using NumCalc.UI.Shared.Resources;

namespace NumCalc.UI.Shared.Models.Auth;

public class ConfirmPasswordFormModel
{
    [Required(ErrorMessageResourceType = typeof(Localization), ErrorMessageResourceName = "PasswordRequired")]
    public string CurrentPassword { get; set; } = string.Empty;
}
