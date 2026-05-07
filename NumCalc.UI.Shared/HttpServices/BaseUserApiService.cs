using System.Net.Http.Headers;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.HttpServices;

public abstract class BaseUserApiService(HttpClient httpClient, IAuthStateService authStateService)
    : BaseApiService(httpClient)
{
    protected override void ConfigureRequest(HttpRequestMessage request)
    {
        if (authStateService.IsAuthenticated)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authStateService.Token);
    }
}
