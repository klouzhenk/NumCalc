using System.Net.Http.Json;
using NumCalc.Shared.Calculation.Requests;
using NumCalc.Shared.Calculation.Responses;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.Services.Implementations;

public class CalculationApiService(HttpClient httpClient) : ICalculationApiService
{
    public async Task<RootFindingResponse?> GetDichotomyResultAsync(RootFindingRequest request)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/rootfinding/dichotomy", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<RootFindingResponse>();
            }

            // TODO: throw custom exception is status is not success (e.g. - ApiException)
        }
        catch (Exception ex)
        {
            throw ex;   
        }
        
        return null;
    }
}