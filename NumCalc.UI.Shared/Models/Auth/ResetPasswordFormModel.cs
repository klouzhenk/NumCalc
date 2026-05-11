using System.ComponentModel.DataAnnotations;
using NumCalc.UI.Shared.Resources;

namespace NumCalc.UI.Shared.Models.Auth;

public class ResetPasswordFormModel : IValidatableObject
{
    [Required(ErrorMessageResourceType = typeof(Localization), ErrorMessageResourceName = "PasswordRequired")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(Localization), ErrorMessageResourceName = "ConfirmPasswordRequired")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (NewPassword != ConfirmPassword)
        {
            var message = Localization.ResourceManager.GetString("PasswordsDoNotMatch") ?? "PasswordsDoNotMatch";
            yield return new ValidationResult(message, [nameof(ConfirmPassword)]);
        }
    }
}
