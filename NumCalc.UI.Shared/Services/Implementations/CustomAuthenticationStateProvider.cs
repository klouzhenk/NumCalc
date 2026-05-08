using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.Services.Implementations;

public sealed class CustomAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly IAuthStateService _authState;
    private readonly TaskCompletionSource _initialized = new();

    public CustomAuthenticationStateProvider(IAuthStateService authState)
    {
        _authState = authState;
        _authState.OnAuthChanged += HandleAuthChanged;

        if (_authState.IsInitialized)
            _initialized.TrySetResult();
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        await _initialized.Task;
        return new AuthenticationState(BuildPrincipal());
    }

    private void HandleAuthChanged()
    {
        _initialized.TrySetResult();
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(BuildPrincipal())));
    }

    private ClaimsPrincipal BuildPrincipal()
    {
        if (!_authState.IsAuthenticated)
            return new ClaimsPrincipal(new ClaimsIdentity());

        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, _authState.Username ?? string.Empty)],
            authenticationType: "jwt");

        return new ClaimsPrincipal(identity);
    }

    public void Dispose()
    {
        if (_authState is not null)
            _authState.OnAuthChanged -= HandleAuthChanged;
    }
}
