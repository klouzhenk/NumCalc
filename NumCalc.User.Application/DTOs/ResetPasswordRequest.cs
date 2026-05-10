using System.ComponentModel.DataAnnotations;

namespace NumCalc.User.Application.DTOs;

public class ResetPasswordRequest
{
    [Required] 
    public string Token { get; set; } = string.Empty;

    [Required]
    public string NewPassword { get; set; } = string.Empty;
}