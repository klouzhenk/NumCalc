using NumCalc.UI.Shared.Models.User;

namespace NumCalc.UI.Shared.HttpServices.Interfaces;

public interface IAuthApiService
{
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
}