using NumCalc.User.Application.DTOs;

namespace NumCalc.User.Application.Interfaces.Services;

public interface IAuthService
{
    Task<UserProfileDto> GetCurrentUserAsync(Guid userId);
    Task UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
    Task DeleteAccountAsync(Guid userId, DeleteAccountRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task RequestPasswordResetAsync(ForgotPasswordRequest request, CancellationToken ct);
    Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct);
}
