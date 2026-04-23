using NumCalc.Shared.Differentiation.Requests;
using NumCalc.Shared.Differentiation.Responses;

namespace NumCalc.Calculation.Api.Services.Interfaces;

public interface IDifferentiationService
{
    DifferentiationResponse SolveForward(DifferentiationRequest request);
    DifferentiationResponse SolveBackward(DifferentiationRequest request);
    DifferentiationResponse SolveCentral(DifferentiationRequest request);
    DifferentiationResponse SolveLagrange(DifferentiationRequest request);
    DifferentiationComparisonResponse Compare(DifferentiationComparisonRequest request);
}
