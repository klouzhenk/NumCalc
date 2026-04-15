using NumCalc.Shared.Differentiation.Requests;
using NumCalc.Shared.Differentiation.Responses;
using NumCalc.Shared.EquationsSystems.Requests;
using NumCalc.Shared.EquationsSystems.Responses;
using NumCalc.Shared.Integration.Requests;
using NumCalc.Shared.Integration.Responses;
using NumCalc.Shared.Optimization.Requests;
using NumCalc.Shared.Optimization.Responses;
using NumCalc.Shared.Interpolation.Requests;
using NumCalc.Shared.Interpolation.Responses;
using NumCalc.Shared.RootFinding.Requests;
using NumCalc.Shared.RootFinding.Responses;
using NumCalc.UI.Shared.HttpServices.Interfaces;

namespace NumCalc.UI.Shared.HttpServices.Implementations;

public class CalculationApiService(HttpClient httpClient) : BaseApiService(httpClient), ICalculationApiService
{
    public async Task<RootFindingResponse?> GetDichotomyResultAsync(RootFindingRequest request)
        => await SendPostRequestAsync<RootFindingResponse>("api/rootfinding/dichotomy", request);

    public async Task<RootFindingResponse?> GetNewtonResultAsync(RootFindingRequest request)
        => await SendPostRequestAsync<RootFindingResponse>("api/rootfinding/newton", request);
    
    public async Task<RootFindingResponse?> GetSimpleIterationsResultAsync(RootFindingRequest request)
        => await SendPostRequestAsync<RootFindingResponse>("api/rootfinding/simple-iterations", request);
    
    public async Task<RootFindingResponse?> GetSecantResultAsync(RootFindingRequest request)
        => await SendPostRequestAsync<RootFindingResponse>("api/rootfinding/secant", request);
    
    public async Task<RootFindingResponse?> GetCombinedResultAsync(RootFindingRequest request)
        => await SendPostRequestAsync<RootFindingResponse>("api/rootfinding/combined", request);

    public async Task<RootFindingComparisonResponse?> GetBenchmarkResultAsync(RootFindingComparisonRequest request)
        => await SendPostRequestAsync<RootFindingComparisonResponse>("api/rootfinding/comparison", request);

    public async Task<SystemSolvingResponse?> SolveCramerAsync(SystemSolvingRequest request)
        => await SendPostRequestAsync<SystemSolvingResponse>("api/equationssystems/cramer", request);

    public async Task<SystemSolvingResponse?> SolveGaussianAsync(SystemSolvingRequest request)
        => await SendPostRequestAsync<SystemSolvingResponse>("api/equationssystems/gaussian", request);

    public async Task<SystemSolvingResponse?> SolveFixedPointAsync(NonLinearSystemRequest request)
        => await SendPostRequestAsync<SystemSolvingResponse>("api/equationssystems/fixed-point", request);
    
    public async Task<SystemSolvingResponse?> SolveSeidelAsync(NonLinearSystemRequest request)
        => await SendPostRequestAsync<SystemSolvingResponse>("api/equationssystems/seidel", request);

    public async Task<InterpolationResponse?> InterpolateNewtonAsync(InterpolationRequest request)
        => await SendPostRequestAsync<InterpolationResponse>("api/interpolation/newton", request);

    public async Task<InterpolationResponse?> InterpolateLagrangeAsync(InterpolationRequest request)
        => await SendPostRequestAsync<InterpolationResponse>("api/interpolation/lagrange", request);

    public async Task<InterpolationResponse?> InterpolateSplineAsync(InterpolationRequest request)
        => await SendPostRequestAsync<InterpolationResponse>("api/interpolation/spline", request);

    public async Task<DifferentiationResponse?> DifferentiateFiniteDiffAsync(DifferentiationRequest request)
        => await SendPostRequestAsync<DifferentiationResponse>("api/differentiation/finite-diff", request);

    public async Task<DifferentiationResponse?> DifferentiateLagrangeAsync(DifferentiationRequest request)
        => await SendPostRequestAsync<DifferentiationResponse>("api/differentiation/lagrange", request);

    public async Task<IntegrationResponse?> IntegrateRectangleAsync(IntegrationRequest request)
        => await SendPostRequestAsync<IntegrationResponse>("api/integration/rectangle", request);

    public async Task<IntegrationResponse?> IntegrateTrapezoidAsync(IntegrationRequest request)
        => await SendPostRequestAsync<IntegrationResponse>("api/integration/trapezoid", request);

    public async Task<IntegrationResponse?> IntegrateSimpsonAsync(IntegrationRequest request)
        => await SendPostRequestAsync<IntegrationResponse>("api/integration/simpson", request);

    public async Task<OptimizationResponse?> OptimizeUniformSearchAsync(OptimizationRequest request)
        => await SendPostRequestAsync<OptimizationResponse>("api/optimization/uniform-search", request);

    public async Task<OptimizationResponse?> OptimizeGoldenSectionAsync(OptimizationRequest request)
        => await SendPostRequestAsync<OptimizationResponse>("api/optimization/golden-section", request);

    public async Task<OptimizationResponse?> OptimizeGradientDescentAsync(GradientDescentRequest request)
        => await SendPostRequestAsync<OptimizationResponse>("api/optimization/gradient-descent", request);
}