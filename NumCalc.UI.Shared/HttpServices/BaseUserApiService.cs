using System.Net.Http.Headers;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.HttpServices;

public abstract class BaseUserApiService : BaseApiService
{
    protected BaseUserApiService(HttpClient httpClient, IAuthStateService authStateService)
        : base(httpClient)
    {
        if (authStateService.IsAuthenticated)
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", authStateService.Token);
    }
}
