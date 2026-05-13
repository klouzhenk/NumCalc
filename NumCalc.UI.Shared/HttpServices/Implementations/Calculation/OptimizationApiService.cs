using NumCalc.Shared.Optimization.Requests;
using NumCalc.Shared.Optimization.Responses;
using NumCalc.UI.Shared.HttpServices.Interfaces.Calculation;

namespace NumCalc.UI.Shared.HttpServices.Implementations.Calculation;

public class OptimizationApiService(HttpClient httpClient) : BaseApiService(httpClient), IOptimizationApiService
{
    protected override string ApiControllerName => "api/optimization";
    
    public async Task<OptimizationResponse?> OptimizeUniformSearchAsync(OptimizationRequest request)
        => await SendPostRequestAsync<OptimizationResponse>($"{ApiControllerName}/uniform-search", request);

    public async Task<OptimizationResponse?> OptimizeGoldenSectionAsync(OptimizationRequest request)
        => await SendPostRequestAsync<OptimizationResponse>($"{ApiControllerName}/golden-section", request);

    public async Task<OptimizationResponse?> OptimizeGradientDescentAsync(GradientDescentRequest request)
        => await SendPostRequestAsync<OptimizationResponse>($"{ApiControllerName}/gradient-descent", request);

    public async Task<OptimizationComparisonResponse?> GetOptimizationComparisonAsync(OptimizationComparisonRequest request)
        => await SendPostRequestAsync<OptimizationComparisonResponse>($"{ApiControllerName}/comparison", request);
}