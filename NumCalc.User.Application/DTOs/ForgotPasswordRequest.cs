using System.ComponentModel.DataAnnotations;

namespace NumCalc.User.Application.DTOs;

public class ForgotPasswordRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}