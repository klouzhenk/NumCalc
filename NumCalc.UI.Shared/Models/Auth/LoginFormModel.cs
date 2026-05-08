using System.ComponentModel.DataAnnotations;

namespace NumCalc.UI.Shared.Models.Auth;

public class LoginFormModel
{
    [Required(ErrorMessage = "UsernameRequired")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "PasswordRequired")]
    public string Password { get; set; } = string.Empty;
}
