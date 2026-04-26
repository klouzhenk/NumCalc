using System.ComponentModel.DataAnnotations;

namespace NumCalc.UI.Shared.Models.Auth;

public class RegisterFormModel : IValidatableObject
{
    [Required(ErrorMessage = "UsernameRequired")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "PasswordRequired")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "ConfirmPasswordRequired")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Password != ConfirmPassword)
            yield return new ValidationResult("PasswordsDoNotMatch", [nameof(ConfirmPassword)]);
    }
}
