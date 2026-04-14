using NumCalc.Shared.Differentiation.Requests;
using NumCalc.Shared.Differentiation.Responses;
using NumCalc.Shared.EquationsSystems.Requests;
using NumCalc.Shared.EquationsSystems.Responses;
using NumCalc.Shared.Interpolation.Requests;
using NumCalc.Shared.Interpolation.Responses;
using NumCalc.Shared.RootFinding.Requests;
using NumCalc.Shared.RootFinding.Responses;

namespace NumCalc.UI.Shared.HttpServices.Interfaces;

public interface ICalculationApiService
{
    Task<RootFindingResponse?> GetDichotomyResultAsync(RootFindingRequest request);
    Task<RootFindingResponse?> GetNewtonResultAsync(RootFindingRequest request);
    Task<RootFindingResponse?> GetSimpleIterationsResultAsync(RootFindingRequest request);
    Task<RootFindingResponse?> GetSecantResultAsync(RootFindingRequest request);
    Task<RootFindingResponse?> GetCombinedResultAsync(RootFindingRequest request);
    Task<RootFindingComparisonResponse?> GetBenchmarkResultAsync(RootFindingComparisonRequest request);
    Task<SystemSolvingResponse?> SolveCramerAsync(SystemSolvingRequest request);
    Task<SystemSolvingResponse?> SolveGaussianAsync(SystemSolvingRequest request);
    Task<SystemSolvingResponse?> SolveFixedPointAsync(NonLinearSystemRequest request);
    Task<SystemSolvingResponse?> SolveSeidelAsync(NonLinearSystemRequest request);
    Task<InterpolationResponse?> InterpolateNewtonAsync(InterpolationRequest request);
    Task<InterpolationResponse?> InterpolateLagrangeAsync(InterpolationRequest request);
    Task<InterpolationResponse?> InterpolateSplineAsync(InterpolationRequest request);
    Task<DifferentiationResponse?> DifferentiateFiniteDiffAsync(DifferentiationRequest request);
    Task<DifferentiationResponse?> DifferentiateLagrangeAsync(DifferentiationRequest request);
}