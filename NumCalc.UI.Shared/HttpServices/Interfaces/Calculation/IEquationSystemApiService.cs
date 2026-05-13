using NumCalc.Shared.EquationsSystems.Requests;
using NumCalc.Shared.EquationsSystems.Responses;

namespace NumCalc.UI.Shared.HttpServices.Interfaces.Calculation;

public interface IEquationSystemApiService
{
    Task<SystemSolvingResponse?> SolveCramerAsync(SystemSolvingRequest request);
    Task<SystemSolvingResponse?> SolveGaussianAsync(SystemSolvingRequest request);
    Task<SystemSolvingResponse?> SolveFixedPointAsync(NonLinearSystemRequest request);
    Task<SystemSolvingResponse?> SolveSeidelAsync(NonLinearSystemRequest request);
    Task<LinearSystemComparisonResponse?> GetLinearComparisonAsync(LinearSystemComparisonRequest request);
    Task<NonLinearSystemComparisonResponse?> GetNonLinearComparisonAsync(NonLinearSystemComparisonRequest request);
}