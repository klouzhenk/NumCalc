using System.Net.Http.Headers;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.HttpServices;

public class AuthorizationHandler(IAuthStateService authStateService) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (authStateService.IsAuthenticated)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authStateService.Token);
        
        return await base.SendAsync(request, cancellationToken);
    }
}