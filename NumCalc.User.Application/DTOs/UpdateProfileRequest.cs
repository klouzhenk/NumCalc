namespace NumCalc.User.Application.DTOs;

public class UpdateProfileRequest
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? NewPassword { get; set; }
    public required string CurrentPassword { get; set; }
}