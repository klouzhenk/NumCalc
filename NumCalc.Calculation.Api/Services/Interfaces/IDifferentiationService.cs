using NumCalc.Shared.Differentiation.Requests;
using NumCalc.Shared.Differentiation.Responses;

namespace NumCalc.Calculation.Api.Services.Interfaces;

public interface IDifferentiationService
{
    DifferentiationResponse SolveFiniteDiff(DifferentiationRequest request);
    DifferentiationResponse SolveLagrange(DifferentiationRequest request);
}