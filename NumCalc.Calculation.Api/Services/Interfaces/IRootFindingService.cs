using NumCalc.Shared.RootFinding.Requests;
using NumCalc.Shared.RootFinding.Responses;

namespace NumCalc.Calculation.Api.Services.Interfaces;

public interface IRootFindingService
{
    RootFindingResponse CalculateDichotomy(RootFindingRequest request);
    RootFindingResponse CalculateNewton(RootFindingRequest request);
    RootFindingResponse CalculateSimpleIterations(RootFindingRequest request);
    RootFindingResponse CalculateSecant(RootFindingRequest request);
    RootFindingResponse CalculateCombined(RootFindingRequest request);
}