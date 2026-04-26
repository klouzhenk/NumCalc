using NumCalc.UI.Shared.Models.User;

namespace NumCalc.UI.Shared.Services.Interfaces;

public interface IAuthStateService
{
    string? Token { get; }
    string? Username { get; }
    bool IsAuthenticated { get; }

    event Action OnAuthChanged;

    void SetAuth(AuthResponse auth);
    void ClearAuth();
}