using System.ComponentModel.DataAnnotations;

namespace NumCalc.UI.Shared.Models.Auth;

public class AccountSettingsFormModel : IValidatableObject
{
    [Required(ErrorMessage = "UsernameRequired")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "EmailRequired")]
    [EmailAddress(ErrorMessage = "EmailIsNotValid")]
    public string Email { get; set; } = string.Empty;

    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmNewPassword { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrEmpty(NewPassword) && NewPassword != ConfirmNewPassword)
            yield return new ValidationResult("PasswordsDoNotMatch", [nameof(ConfirmNewPassword)]);
    }
}
