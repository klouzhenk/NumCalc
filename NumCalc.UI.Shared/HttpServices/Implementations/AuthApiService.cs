using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.User;

namespace NumCalc.UI.Shared.HttpServices.Implementations;

public class AuthApiService(HttpClient httpClient) : BaseApiService(httpClient), IAuthApiService
{
    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request) =>
        await SendPostRequestAsync<AuthResponse>("api/auth/register", request);

    public async Task<AuthResponse?> LoginAsync(LoginRequest request) =>
        await SendPostRequestAsync<AuthResponse>("api/auth/login", request);
}