using System.ComponentModel.DataAnnotations;
using NumCalc.UI.Shared.Resources;

namespace NumCalc.UI.Shared.Models.Auth;

public class ForgotPasswordFormModel
{
    [Required(ErrorMessageResourceType = typeof(Localization), ErrorMessageResourceName = "EmailRequired")]
    [EmailAddress(ErrorMessageResourceType = typeof(Localization), ErrorMessageResourceName = "EmailIsNotValid")]
    public string Email { get; set; } = string.Empty;
}
