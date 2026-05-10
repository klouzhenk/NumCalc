using System.ComponentModel.DataAnnotations;

namespace NumCalc.UI.Shared.Models.Auth;

public class ForgotPasswordFormModel
{
    [Required(ErrorMessage = "EmailRequired")]
    [EmailAddress(ErrorMessage = "EmailIsNotValid")]
    public string Email { get; set; } = string.Empty;
}