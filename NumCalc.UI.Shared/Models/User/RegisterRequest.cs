using System.ComponentModel.DataAnnotations;

namespace NumCalc.UI.Shared.Models.User;

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    [Required] [EmailAddress] public string Email { get; set; } = string.Empty;
}
