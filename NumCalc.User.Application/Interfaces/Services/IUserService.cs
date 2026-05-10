using NumCalc.User.Application.DTOs;

namespace NumCalc.User.Application.Interfaces.Services;

public interface IUserService
{
    Task<UserProfileDto> GetCurrentUserAsync(Guid userId);
    Task UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
    Task DeleteAccountAsync(Guid userId, DeleteAccountRequest request);
}