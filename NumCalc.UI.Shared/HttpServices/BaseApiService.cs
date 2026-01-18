using System.Net.Http.Json;
using System.Text.Json;

namespace NumCalc.UI.Shared.HttpServices;

public abstract class BaseApiService(HttpClient httpClient)
{
    protected async Task<TResponse?> SendPostRequestAsync<TResponse>(string endpoint, object requestData)
    {
        var response = await httpClient.PostAsJsonAsync(endpoint, requestData);
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<TResponse>() 
                   ?? throw new ApplicationException("EMPTY_SERVER_RESPONSE");
        }
        
        var errorMessage = "UNKNOWN_SERVER_ERROR";
        var errorContent = await response.Content.ReadAsStringAsync();

        try
        {
            var problemDetails = JsonSerializer.Deserialize<JsonElement>(errorContent);
            
            if (problemDetails.TryGetProperty("detail", out var detail))
            {
                errorMessage = detail.GetString() ?? errorMessage;
            }
            else if (problemDetails.TryGetProperty("title", out var title))
            {
                errorMessage = title.GetString() ?? errorMessage;
            }
            else if (problemDetails.TryGetProperty("error", out var error))
            {
                errorMessage = error.GetString() ?? errorMessage;
            }
        }
        catch
        {
            errorMessage = $"HTTP_ERROR_{response.StatusCode}";
        }
        
        throw new ApplicationException(errorMessage);
    }
}