using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using NumCalc.Calculation.Business.Exceptions;
using NumCalc.Calculation.Business.Services.Interfaces;
using NumCalc.Shared.Enums;

namespace NumCalc.Calculation.Business.Services.Implementations;

public class GeminiOcrProvider(HttpClient httpClient, IConfiguration configuration) : IOcrProvider
{
    private const string Endpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";
    private const string Prompt =
        "Scan this image and output ONLY the raw LaTeX code for the mathematical formula. " +
        "Do not use markdown blocks. DO NOT wrap the formula in $ or $$ signs.";
    
    private readonly string _apiKey = configuration["OcrSettings:GeminiApiKey"]?.Trim()
            ?? throw new CustomException(NumCalcErrorCode.InvalidData, "OCR API key is not configured");
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<string> RecognizeLatexAsync(string base64Image, string mimeType, CancellationToken ct = default)
    {
        var payload = new GeminiRequest([
            new GeminiContent([
                new GeminiPart(Text: Prompt),
                new GeminiPart(InlineData: new GeminiInlineData(mimeType, base64Image))
            ])
        ]);
        
        var response = await httpClient.PostAsJsonAsync($"{Endpoint}?key={_apiKey}", payload, JsonOptions, ct);
        response.EnsureSuccessStatusCode();
        
        var body = await response.Content.ReadFromJsonAsync<GeminiResponse>(JsonOptions, ct);
        return body?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? string.Empty;
    }
    
    private sealed record GeminiRequest(GeminiContent[] Contents);
    private sealed record GeminiContent(GeminiPart[] Parts);
    private sealed record GeminiPart(string? Text = null, GeminiInlineData? InlineData = null);
    private sealed record GeminiInlineData(string MimeType, string Data);

    private sealed record GeminiResponse(GeminiCandidate[]? Candidates);
    private sealed record GeminiCandidate(GeminiContent? Content);
}