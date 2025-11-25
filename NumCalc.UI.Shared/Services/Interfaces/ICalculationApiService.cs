using NumCalc.Shared.Calculation.Requests;
using NumCalc.Shared.Calculation.Responses;

namespace NumCalc.UI.Shared.Services.Interfaces;

public interface ICalculationApiService
{
    Task<RootFindingResponse?> GetDichotomyResultAsync(RootFindingRequest request);
}