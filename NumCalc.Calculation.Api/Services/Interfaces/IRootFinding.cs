using NumCalc.Shared.Calculation.Requests;
using NumCalc.Shared.Calculation.Responses;

namespace NumCalc.Calculation.Api.Services.Interfaces;

public interface IRootFinding
{
    RootFindingResponse CalculateDichotomy(RootFindingRequest request);
}