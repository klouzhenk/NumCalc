using NumCalc.Shared.Calculation.Requests;
using NumCalc.Shared.Calculation.Responses;
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

    public Task<RootFindingResponse?> GetSelectedMethodResultAsync(RootFindingRequest request)
    {
        throw new NotImplementedException();
    }
}