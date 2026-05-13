using NumCalc.Shared.RootFinding.Requests;
using NumCalc.Shared.RootFinding.Responses;
using NumCalc.UI.Shared.HttpServices.Interfaces.Calculation;

namespace NumCalc.UI.Shared.HttpServices.Implementations.Calculation;

public class RootFindingApiService(HttpClient httpClient) : BaseApiService(httpClient), IRootFindingApiService
{
    protected override string ApiControllerName => "api/rootfinding";
    
    public async Task<RootFindingResponse?> GetDichotomyResultAsync(RootFindingRequest request)
        => await SendPostRequestAsync<RootFindingResponse>($"{ApiControllerName}/dichotomy", request);

    public async Task<RootFindingResponse?> GetNewtonResultAsync(RootFindingRequest request)
        => await SendPostRequestAsync<RootFindingResponse>($"{ApiControllerName}/newton", request);
    
    public async Task<RootFindingResponse?> GetSimpleIterationsResultAsync(RootFindingRequest request)
        => await SendPostRequestAsync<RootFindingResponse>($"{ApiControllerName}/simple-iterations", request);
    
    public async Task<RootFindingResponse?> GetSecantResultAsync(RootFindingRequest request)
        => await SendPostRequestAsync<RootFindingResponse>($"{ApiControllerName}/secant", request);
    
    public async Task<RootFindingResponse?> GetCombinedResultAsync(RootFindingRequest request)
        => await SendPostRequestAsync<RootFindingResponse>($"{ApiControllerName}/combined", request);

    public async Task<RootFindingComparisonResponse?> GetBenchmarkResultAsync(RootFindingComparisonRequest request)
        => await SendPostRequestAsync<RootFindingComparisonResponse>($"{ApiControllerName}/comparison", request);
}