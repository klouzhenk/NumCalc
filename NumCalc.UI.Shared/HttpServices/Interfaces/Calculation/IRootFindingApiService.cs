using NumCalc.Shared.RootFinding.Requests;
using NumCalc.Shared.RootFinding.Responses;

namespace NumCalc.UI.Shared.HttpServices.Interfaces.Calculation;

public interface IRootFindingApiService
{
    Task<RootFindingResponse?> GetDichotomyResultAsync(RootFindingRequest request);
    Task<RootFindingResponse?> GetNewtonResultAsync(RootFindingRequest request);
    Task<RootFindingResponse?> GetSimpleIterationsResultAsync(RootFindingRequest request);
    Task<RootFindingResponse?> GetSecantResultAsync(RootFindingRequest request);
    Task<RootFindingResponse?> GetCombinedResultAsync(RootFindingRequest request);
    Task<RootFindingComparisonResponse?> GetBenchmarkResultAsync(RootFindingComparisonRequest request);
}