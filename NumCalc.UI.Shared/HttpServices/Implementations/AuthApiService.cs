using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.User;
using ForgotPasswordRequest = NumCalc.UI.Shared.Models.User.ForgotPasswordRequest;
using LoginRequest = NumCalc.UI.Shared.Models.User.LoginRequest;
using RegisterRequest = NumCalc.UI.Shared.Models.User.RegisterRequest;

namespace NumCalc.UI.Shared.HttpServices.Implementations;

public class AuthApiService(HttpClient httpClient) : BaseApiService(httpClient), IAuthApiService
{
    protected override string ApiControllerName => "api/auth";

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request) =>
        await SendPostRequestAsync<AuthResponse>($"{ApiControllerName}/register", request);

    public async Task<AuthResponse?> LoginAsync(LoginRequest request) =>
        await SendPostRequestAsync<AuthResponse>($"{ApiControllerName}/login", request);
    
    public async Task ForgotPasswordAsync(ForgotPasswordRequest request) =>
        await SendPostRequestAsync($"{ApiControllerName}/forgot-password", request);

    public async Task ResetPasswordAsync(ResetPasswordRequest request) =>
        await SendPostRequestAsync($"{ApiControllerName}/reset-password", request);
}