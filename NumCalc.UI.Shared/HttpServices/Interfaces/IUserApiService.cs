using NumCalc.UI.Shared.Models.User;

namespace NumCalc.UI.Shared.HttpServices.Interfaces;

public interface IUserApiService
{
    Task<UserProfileDto?> GetCurrentUserAsync();
    Task UpdateProfileAsync(UpdateProfileRequest request);
    Task DeleteAccountAsync(DeleteAccountRequest request);
}
