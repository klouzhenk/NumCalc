using NumCalc.Shared.EquationsSystems.Requests;
using NumCalc.Shared.EquationsSystems.Responses;
using NumCalc.UI.Shared.HttpServices.Interfaces.Calculation;

namespace NumCalc.UI.Shared.HttpServices.Implementations.Calculation;

public class EquationSystemApiService(HttpClient httpClient) : BaseApiService(httpClient), IEquationSystemApiService 
{
    protected override string ApiControllerName => "api/equationssystems";
    
    public async Task<SystemSolvingResponse?> SolveCramerAsync(SystemSolvingRequest request)
        => await SendPostRequestAsync<SystemSolvingResponse>($"{ApiControllerName}/cramer", request);

    public async Task<SystemSolvingResponse?> SolveGaussianAsync(SystemSolvingRequest request)
        => await SendPostRequestAsync<SystemSolvingResponse>($"{ApiControllerName}/gaussian", request);

    public async Task<SystemSolvingResponse?> SolveFixedPointAsync(NonLinearSystemRequest request)
        => await SendPostRequestAsync<SystemSolvingResponse>($"{ApiControllerName}/fixed-point", request);

    public async Task<SystemSolvingResponse?> SolveSeidelAsync(NonLinearSystemRequest request)
        => await SendPostRequestAsync<SystemSolvingResponse>($"{ApiControllerName}/seidel", request);

    public async Task<LinearSystemComparisonResponse?> GetLinearComparisonAsync(LinearSystemComparisonRequest request)
        => await SendPostRequestAsync<LinearSystemComparisonResponse>($"{ApiControllerName}/linear-comparison", request);

    public async Task<NonLinearSystemComparisonResponse?> GetNonLinearComparisonAsync(NonLinearSystemComparisonRequest request)
        => await SendPostRequestAsync<NonLinearSystemComparisonResponse>($"{ApiControllerName}/nonlinear-comparison", request);
}