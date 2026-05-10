using System.ComponentModel.DataAnnotations;

namespace NumCalc.UI.Shared.Models.Auth;

public class ConfirmPasswordFormModel
{
    [Required(ErrorMessage = "PasswordRequired")]
    public string CurrentPassword { get; set; } = string.Empty;
}
