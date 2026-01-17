using System.Net.Http.Json;
using System.Text.Json;
using NumCalc.Shared.Calculation.Requests;
using NumCalc.Shared.Calculation.Responses;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.Services.Implementations;

public class CalculationApiService(HttpClient httpClient) : ICalculationApiService
{
    public async Task<RootFindingResponse?> GetDichotomyResultAsync(RootFindingRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/rootfinding/dichotomy", request);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<RootFindingResponse>() 
                   ?? throw new Exception("Empty response from server");
        }

        var errorContent = await response.Content.ReadAsStringAsync();
        var errorMessage = "Unknown error occurred";
        
        try 
        {
            var problemDetails = JsonSerializer.Deserialize<JsonElement>(errorContent);
            if (problemDetails.TryGetProperty("detail", out var detail))
                errorMessage = detail.GetString() ?? errorMessage;
        }
        catch
        {
            errorMessage = $"Error {response.StatusCode}: {errorContent}";
        }

        throw new ApplicationException(errorMessage);
    }
}