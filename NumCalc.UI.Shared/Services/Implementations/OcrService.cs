using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.Services.Implementations;

public class OcrService : IOcrService
{
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;
    
    public OcrService(HttpClient httpClient, ILogger<OcrService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["OcrSettings:GeminiApiKey"];
        // TODO : use try-catch
    }
    
    public async Task<string> RecognizeExpression(IBrowserFile file)
    {
        using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        var base64Image = Convert.ToBase64String(ms.ToArray());
        var mimeType = file.ContentType;
        
        try 
        {
            if (!string.IsNullOrEmpty(_apiKey))
            {
                return await CallGeminiApi(base64Image, mimeType);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Gemini failed: {ex.Message}. Trying fallback...");
        }

        return "RECOGNIZING_ERROR";
    }
    
    public async Task<string> RecognizeExpression(string? imageBase64DataUrl)
    {
        try 
        {
            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(imageBase64DataUrl))
                return "RECOGNIZING_ERROR";

            var parts = imageBase64DataUrl.Split(',');
            if (parts.Length != 2)
            {
                return "RECOGNIZING_ERROR";
            }

            var header = parts[0];
            var base64Image = parts[1];
            var mimeType = header.Replace("data:", "").Replace(";base64", "");

            return await CallGeminiApi(base64Image, mimeType);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Gemini failed: {ex.Message}. Trying fallback...");
            return "RECOGNIZING_ERROR";
        }
    }

    private async Task<string> CallGeminiApi(string base64Image, string mimeType)
    {
        var cleanKey = _apiKey?.Trim();
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={cleanKey}";

        var payload = new
        {
            contents = new[]
            {
                new
                {
                    parts = new object[]
                    {
                        new
                        {
                            text = "Scan this image and output ONLY the raw LaTeX code for the mathematical formula. Do not use markdown blocks. DO NOT wrap the formula in $ or $$ signs."
                        },
                        new
                        {
                            inlineData = new 
                            {
                                mimeType = mimeType, 
                                data = base64Image
                            }
                        }
                    }
                }
            }
        };

        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
    
        var response = await _httpClient.PostAsync(url, content);
    
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Gemini API Error: {response.StatusCode} | {errorBody}");
        }
    
        var responseString = await response.Content.ReadAsStringAsync();
        var node = JsonNode.Parse(responseString);
    
        var text = node?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();
    
        return text?.Replace("```latex", "")
            .Replace("```", "")
            .Replace("$", "")
            .Replace("\n", "")
            .Trim() 
            .ToLower() ?? "";
    }
}