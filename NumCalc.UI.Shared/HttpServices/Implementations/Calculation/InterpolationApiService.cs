using NumCalc.Shared.Interpolation.Requests;
using NumCalc.Shared.Interpolation.Responses;
using NumCalc.UI.Shared.HttpServices.Interfaces.Calculation;

namespace NumCalc.UI.Shared.HttpServices.Implementations.Calculation;

public class InterpolationApiService(HttpClient httpClient) : BaseApiService(httpClient), IInterpolationApiService
{
    protected override string ApiControllerName => "api/interpolation";

    public async Task<InterpolationResponse?> InterpolateNewtonAsync(InterpolationRequest request)
        => await SendPostRequestAsync<InterpolationResponse>($"{ApiControllerName}/newton", request);

    public async Task<InterpolationResponse?> InterpolateLagrangeAsync(InterpolationRequest request)
        => await SendPostRequestAsync<InterpolationResponse>($"{ApiControllerName}/lagrange", request);

    public async Task<InterpolationResponse?> InterpolateSplineAsync(InterpolationRequest request)
        => await SendPostRequestAsync<InterpolationResponse>($"{ApiControllerName}/spline", request);

    public async Task<InterpolationComparisonResponse?> GetInterpolationComparisonAsync(InterpolationComparisonRequest request)
        => await SendPostRequestAsync<InterpolationComparisonResponse>($"{ApiControllerName}/comparison", request);
}