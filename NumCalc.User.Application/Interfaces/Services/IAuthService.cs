using NumCalc.Shared.User.Requests;
using NumCalc.Shared.User.Responses;

namespace NumCalc.User.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}
