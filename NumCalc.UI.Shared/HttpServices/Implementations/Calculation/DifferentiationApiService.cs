using NumCalc.Shared.Differentiation.Requests;
using NumCalc.Shared.Differentiation.Responses;
using NumCalc.Shared.Enums.Differentiation;
using NumCalc.UI.Shared.HttpServices.Interfaces.Calculation;

namespace NumCalc.UI.Shared.HttpServices.Implementations.Calculation;

public class DifferentiationApiService(HttpClient httpClient) : BaseApiService(httpClient), IDifferentiationApiService
{
    protected override string ApiControllerName => "api/differentiation";
    
    public async Task<DifferentiationResponse?> DifferentiateFiniteDiffAsync(DifferentiationRequest request, FiniteDiffVariant variant)
        => await SendPostRequestAsync<DifferentiationResponse>($"{ApiControllerName}/finite-diff?variant={variant}", request);

    public async Task<DifferentiationResponse?> DifferentiateLagrangeAsync(DifferentiationRequest request)
        => await SendPostRequestAsync<DifferentiationResponse>($"{ApiControllerName}/lagrange", request);

    public async Task<DifferentiationComparisonResponse?> GetDifferentiationComparisonAsync(DifferentiationComparisonRequest request)
        => await SendPostRequestAsync<DifferentiationComparisonResponse>($"{ApiControllerName}/comparison", request);
}