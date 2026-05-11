using System.ComponentModel.DataAnnotations;
using NumCalc.UI.Shared.Resources;

namespace NumCalc.UI.Shared.Models.Auth;

public class LoginFormModel
{
    [Required(ErrorMessageResourceType = typeof(Localization), ErrorMessageResourceName = "UsernameRequired")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(Localization), ErrorMessageResourceName = "PasswordRequired")]
    public string Password { get; set; } = string.Empty;
}
