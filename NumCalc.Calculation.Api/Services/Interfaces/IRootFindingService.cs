using NumCalc.Shared.Calculation.Requests;
using NumCalc.Shared.Calculation.Responses;

namespace NumCalc.Calculation.Api.Services.Interfaces;

public interface IRootFindingService
{
    RootFindingResponse CalculateDichotomy(RootFindingRequest request);
}