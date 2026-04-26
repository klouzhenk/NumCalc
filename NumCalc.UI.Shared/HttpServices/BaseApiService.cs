using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using NumCalc.UI.Shared.Exceptions;

namespace NumCalc.UI.Shared.HttpServices;

public abstract class BaseApiService(HttpClient httpClient)
{
    protected readonly HttpClient HttpClient = httpClient;
    
    protected async Task<TResponse?> SendPostRequestAsync<TResponse>(string endpoint, object requestData)
    {
        var response = await HttpClient.PostAsJsonAsync(endpoint, requestData);
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<TResponse>() 
                   ?? throw new ApiException("EMPTY_SERVER_RESPONSE");
        }
        
        var errorMessage = await ExtractErrorMessageAsync(response);
        throw new ApiException(errorMessage);
    }
    
    protected async Task SendPostRequestAsync(string endpoint, object requestData)
    {
        var response = await HttpClient.PostAsJsonAsync(endpoint, requestData);
        
        if (response.IsSuccessStatusCode)
            return;
        
        var errorMessage = await ExtractErrorMessageAsync(response);
        throw new ApiException(errorMessage);
    }
    
    protected async Task<TResponse?> SendGetRequestAsync<TResponse>(string endpoint, Dictionary<string, string>? queryParams = null)
    {
        if (queryParams is { Count: > 0})
        {
            var queryString = QueryString.Create(queryParams!);
            endpoint += queryString.Value;
        }
        
        var response = await HttpClient.GetAsync(endpoint);
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<TResponse>() 
                ?? throw new ApiException("EMPTY_SERVER_RESPONSE");
        }        
        
        var errorMessage = await ExtractErrorMessageAsync(response);
        throw new ApiException(errorMessage);
    }
    
    protected async Task SendDeleteRequestAsync(string endpoint, Dictionary<string, string>? queryParams = null)
    {
        if (queryParams is { Count: > 0})
        {
            var queryString = QueryString.Create(queryParams!);
            endpoint += queryString.Value;
        }
        
        var response = await HttpClient.DeleteAsync(endpoint);
        
        if (response.IsSuccessStatusCode)
            return;
        
        var errorMessage = await ExtractErrorMessageAsync(response);
        throw new ApiException(errorMessage);
    }

    protected static async Task<string> ExtractErrorMessageAsync(HttpResponseMessage response)
    {
        var errorMessage = "UNKNOWN_SERVER_ERROR";
        var errorContent = await response.Content.ReadAsStringAsync();

        try
        {
            using var document = JsonDocument.Parse(errorContent);
            var problemDetails = document.RootElement;
            
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

        return errorMessage;
    }
}