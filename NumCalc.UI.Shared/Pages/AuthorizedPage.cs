using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace NumCalc.UI.Shared.Pages;

public abstract class AuthorizedPage<TPageType> : BasePage<TPageType>
{
    [CascadingParameter] private Task<AuthenticationState>? AuthStateTask { get; set; }

    protected sealed override async Task OnInitializedAsync()
    {
        if (AuthStateTask is null) return;

        var state = await AuthStateTask;
        if (state.User.Identity?.IsAuthenticated == true)
            await OnAuthenticatedInitAsync();
    }

    protected virtual Task OnAuthenticatedInitAsync() => Task.CompletedTask;
}
