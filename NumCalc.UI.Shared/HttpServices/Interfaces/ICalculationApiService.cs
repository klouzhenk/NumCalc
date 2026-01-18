using NumCalc.Shared.Calculation.Requests;
using NumCalc.Shared.Calculation.Responses;

namespace NumCalc.UI.Shared.HttpServices.Interfaces;

public interface ICalculationApiService
{
    Task<RootFindingResponse?> GetDichotomyResultAsync(RootFindingRequest request);
    Task<RootFindingResponse?> GetNewtonResultAsync(RootFindingRequest request);
}