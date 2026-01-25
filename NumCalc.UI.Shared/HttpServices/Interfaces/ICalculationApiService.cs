using NumCalc.Shared.Calculation.Requests;
using NumCalc.Shared.Calculation.Responses;

namespace NumCalc.UI.Shared.HttpServices.Interfaces;

public interface ICalculationApiService
{
    Task<RootFindingResponse?> GetDichotomyResultAsync(RootFindingRequest request);
    Task<RootFindingResponse?> GetNewtonResultAsync(RootFindingRequest request);
    Task<RootFindingResponse?> GetSimpleIterationsResultAsync(RootFindingRequest request);
    Task<RootFindingResponse?> GetSecantResultAsync(RootFindingRequest request);
    Task<RootFindingResponse?> GetCombinedResultAsync(RootFindingRequest request);
    Task<RootFindingResponse?> GetSelectedMethodResultAsync(RootFindingRequest request);
}