using NumCalc.Shared.ODE.Requests;
using NumCalc.Shared.ODE.Responses;
using NumCalc.UI.Shared.HttpServices.Interfaces.Calculation;

namespace NumCalc.UI.Shared.HttpServices.Implementations.Calculation;

public class OdeApiService(HttpClient httpClient) : BaseApiService(httpClient), IOdeApiService
{
    protected override string ApiControllerName => "api/ode";

    public async Task<OdeResponse?> SolveEuler(OdeRequest request)
        => await SendPostRequestAsync<OdeResponse>($"{ApiControllerName}/euler", request);
    
    public async Task<OdeResponse?> SolveEulerImproved(OdeRequest request)
        => await SendPostRequestAsync<OdeResponse>($"{ApiControllerName}/euler-improved", request);

    public async Task<OdeResponse?> SolveRungeKutta2(OdeRequest request)
        => await SendPostRequestAsync<OdeResponse>($"{ApiControllerName}/runge-kutta-2", request);

    public async Task<OdeResponse?> SolveRungeKutta4(OdeRequest request)
        => await SendPostRequestAsync<OdeResponse>($"{ApiControllerName}/runge-kutta-4", request);
    
    public async Task<OdeResponse?> SolvePicard(OdeRequest request)
        => await SendPostRequestAsync<OdeResponse>($"{ApiControllerName}/picard", request);

    public async Task<OdeComparisonResponse?> GetOdeComparisonAsync(OdeComparisonRequest request)
        => await SendPostRequestAsync<OdeComparisonResponse>($"{ApiControllerName}/comparison", request);
}