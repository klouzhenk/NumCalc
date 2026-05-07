using NumCalc.UI.Shared.Models.User;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.Services.Implementations;

public class AuthStateService : IAuthStateService
{
    public string? Token { get; private set; }
    public string? Username { get; private set; }

    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);
    public bool IsInitialized { get; private set; }

    public event Action? OnAuthChanged;

    public void SetAuth(AuthResponse auth)
    {
        Token = auth.Token;
        Username = auth.Username;
        IsInitialized = true;
        OnAuthChanged?.Invoke();
    }

    public void ClearAuth()
    {
        Token = null;
        Username = null;
        IsInitialized = true;
        OnAuthChanged?.Invoke();
    }

    public void MarkInitialized()
    {
        if (IsInitialized) return;
        IsInitialized = true;
        OnAuthChanged?.Invoke();
    }
}