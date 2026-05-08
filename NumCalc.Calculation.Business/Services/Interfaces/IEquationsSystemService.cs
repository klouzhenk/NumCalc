using NumCalc.Shared.EquationsSystems.Requests;
using NumCalc.Shared.EquationsSystems.Responses;

namespace NumCalc.Calculation.Business.Services.Interfaces;

public interface IEquationsSystemService
{
    SystemSolvingResponse SolveCramer(SystemSolvingRequest model);
    SystemSolvingResponse SolveGaussian(SystemSolvingRequest model);
    SystemSolvingResponse SolveFixedPoint(NonLinearSystemRequest model);
    SystemSolvingResponse SolveSeidel(NonLinearSystemRequest model);
    LinearSystemComparisonResponse CompareLinear(LinearSystemComparisonRequest request);
    NonLinearSystemComparisonResponse CompareNonLinear(NonLinearSystemComparisonRequest request);
}