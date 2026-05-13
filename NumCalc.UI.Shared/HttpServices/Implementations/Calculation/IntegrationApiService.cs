using NumCalc.Shared.Integration.Requests;
using NumCalc.Shared.Integration.Responses;
using NumCalc.UI.Shared.HttpServices.Interfaces.Calculation;

namespace NumCalc.UI.Shared.HttpServices.Implementations.Calculation;

public class IntegrationApiService(HttpClient httpClient) : BaseApiService(httpClient), IIntegrationApiService
{
    protected override string ApiControllerName => "api/integration";
    
    public async Task<IntegrationResponse?> IntegrateRectangleAsync(IntegrationRequest request)
        => await SendPostRequestAsync<IntegrationResponse>($"{ApiControllerName}/rectangle", request);

    public async Task<IntegrationResponse?> IntegrateTrapezoidAsync(IntegrationRequest request)
        => await SendPostRequestAsync<IntegrationResponse>($"{ApiControllerName}/trapezoid", request);

    public async Task<IntegrationResponse?> IntegrateSimpsonAsync(IntegrationRequest request)
        => await SendPostRequestAsync<IntegrationResponse>($"{ApiControllerName}/simpson", request);

    public async Task<IntegrationComparisonResponse?> GetIntegrationComparisonAsync(IntegrationComparisonRequest request)
        => await SendPostRequestAsync<IntegrationComparisonResponse>($"{ApiControllerName}/comparison", request);
}