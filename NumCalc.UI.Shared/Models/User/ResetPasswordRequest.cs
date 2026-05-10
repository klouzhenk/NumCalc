using System.ComponentModel.DataAnnotations;

namespace NumCalc.UI.Shared.Models.User;

public class ResetPasswordRequest
{
    [Required] 
    public string Token { get; set; } = string.Empty;

    [Required]
    public string NewPassword { get; set; } = string.Empty;
}