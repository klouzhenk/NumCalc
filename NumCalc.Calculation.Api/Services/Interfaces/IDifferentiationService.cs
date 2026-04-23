using NumCalc.Shared.Differentiation.Requests;
using NumCalc.Shared.Differentiation.Responses;
using NumCalc.Shared.Enums.Differentiation;

namespace NumCalc.Calculation.Api.Services.Interfaces;

public interface IDifferentiationService
{
    DifferentiationResponse SolveFiniteDiff(DifferentiationRequest request, FiniteDiffVariant variant);
    DifferentiationResponse SolveLagrange(DifferentiationRequest request);
    DifferentiationComparisonResponse Compare(DifferentiationComparisonRequest request);
}
