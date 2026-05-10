using System.ComponentModel.DataAnnotations;

namespace NumCalc.UI.Shared.Models.User;

public class ForgotPasswordRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}