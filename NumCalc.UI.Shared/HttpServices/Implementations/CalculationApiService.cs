using NumCalc.Shared.Enums.RootFinding;
using NumCalc.Shared.RootFinding.Requests;
using NumCalc.Shared.RootFinding.Responses;
using NumCalc.UI.Shared.Enums.Roots;
using NumCalc.UI.Shared.HttpServices.Interfaces;

namespace NumCalc.UI.Shared.HttpServices.Implementations;

public class CalculationApiService(HttpClient httpClient) : BaseApiService(httpClient), ICalculationApiService
{
    public async Task<RootFindingResponse?> GetDichotomyResultAsync(RootFindingRequest request)
        => await SendPostRequestAsync<RootFindingResponse>("api/rootfinding/dichotomy", request);

    public async Task<RootFindingResponse?> GetNewtonResultAsync(RootFindingRequest request)
        => await SendPostRequestAsync<RootFindingResponse>("api/rootfinding/newton", request);
    
    public async Task<RootFindingResponse?> GetSimpleIterationsResultAsync(RootFindingRequest request)
        => await SendPostRequestAsync<RootFindingResponse>("api/rootfinding/simple-iterations", request);
    
    public async Task<RootFindingResponse?> GetSecantResultAsync(RootFindingRequest request)
        => await SendPostRequestAsync<RootFindingResponse>("api/rootfinding/secant", request);
    
    public async Task<RootFindingResponse?> GetCombinedResultAsync(RootFindingRequest request)
        => await SendPostRequestAsync<RootFindingResponse>("api/rootfinding/combined", request);

    public async Task<RootFindingComparisonResponse?> GetBenchmarkResultAsync(RootFindingComparisonRequest request)
        => await SendPostRequestAsync<RootFindingComparisonResponse>("api/rootfinding/comparison", request);
}