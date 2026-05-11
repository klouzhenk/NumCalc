using System.ComponentModel.DataAnnotations;
using NumCalc.UI.Shared.Resources;

namespace NumCalc.UI.Shared.Models.Auth;

public class RegisterFormModel : IValidatableObject
{
    [Required(ErrorMessageResourceType = typeof(Localization), ErrorMessageResourceName = "UsernameRequired")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(Localization), ErrorMessageResourceName = "EmailRequired")]
    [EmailAddress(ErrorMessageResourceType = typeof(Localization), ErrorMessageResourceName = "EmailIsNotValid")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(Localization), ErrorMessageResourceName = "PasswordRequired")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(Localization), ErrorMessageResourceName = "ConfirmPasswordRequired")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Password != ConfirmPassword)
        {
            var message = Localization.ResourceManager.GetString("PasswordsDoNotMatch") ?? "PasswordsDoNotMatch";
            yield return new ValidationResult(message, [nameof(ConfirmPassword)]);
        }
    }
}
