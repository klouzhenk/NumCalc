using NumCalc.Shared.Calculation.Requests;
using NumCalc.Shared.Calculation.Responses;
using NumCalc.UI.Shared.Enums.Roots;

namespace NumCalc.UI.Shared.HttpServices.Interfaces;

public interface ICalculationApiService
{
    Task<RootFindingResponse?> GetDichotomyResultAsync(RootFindingRequest request);
    Task<RootFindingResponse?> GetNewtonResultAsync(RootFindingRequest request);
    Task<RootFindingResponse?> GetSimpleIterationsResultAsync(RootFindingRequest request);
    Task<RootFindingResponse?> GetSecantResultAsync(RootFindingRequest request);
    Task<RootFindingResponse?> GetCombinedResultAsync(RootFindingRequest request);
    Task<RootFindingResponse?> GetBenchmarkResultAsync(RootFindingRequest request, IEnumerable<RootFindingMethod> selectedMethods);
}