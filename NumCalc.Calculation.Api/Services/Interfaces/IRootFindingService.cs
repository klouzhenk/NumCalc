using NumCalc.Shared.Calculation.Requests;
using NumCalc.Shared.Calculation.Responses;

namespace NumCalc.Calculation.Api.Services.Interfaces;

public interface IRootFindingService
{
    RootFindingResponse CalculateDichotomy(RootFindingRequest request);
    RootFindingResponse CalculateNewton(RootFindingRequest request);
    RootFindingResponse CalculateSimpleIterations(RootFindingRequest request);
    RootFindingResponse CalculateSecant(RootFindingRequest request);
    RootFindingResponse CalculateCombined(RootFindingRequest request);
}