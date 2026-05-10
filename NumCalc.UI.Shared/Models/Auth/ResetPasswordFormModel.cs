using System.ComponentModel.DataAnnotations;

namespace NumCalc.UI.Shared.Models.Auth;

public class ResetPasswordFormModel : IValidatableObject
{
    [Required(ErrorMessage = "PasswordRequired")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "ConfirmPasswordRequired")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (NewPassword != ConfirmPassword)
            yield return new ValidationResult("PasswordsDoNotMatch", [nameof(ConfirmPassword)]);
    }
}