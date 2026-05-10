using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.User;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.HttpServices.Implementations;

public class UserApiService(HttpClient httpClient, IAuthStateService authStateService)
    : BaseUserApiService(httpClient, authStateService), IUserApiService
{
    public async Task<UserProfileDto?> GetCurrentUserAsync() =>
        await SendGetRequestAsync<UserProfileDto>("api/user");

    public async Task UpdateProfileAsync(UpdateProfileRequest request) =>
        await SendPatchRequestAsync("api/user", request);

    public async Task DeleteAccountAsync(DeleteAccountRequest request) =>
        await SendDeleteRequestAsync("api/user", request);
}
