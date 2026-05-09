using Microsoft.Extensions.Logging;
using NumCalc.Calculation.Business.Exceptions;
using NumCalc.Calculation.Business.Services.Interfaces;
using NumCalc.Shared.Enums;
using NumCalc.Shared.OCR.Requests;
using NumCalc.Shared.OCR.Responses;

namespace NumCalc.Calculation.Business.Services.Implementations;

public class OcrService(ILogger<OcrService> logger, IOcrProvider ocrProvider) : IOcrService
{
    public async Task<OcrResponse> RecognizeAsync(OcrRequest request, CancellationToken ct = default)
    {
        var (base64Image, mimeType) = ParseDataUrl(request);
        logger.LogInformation("OCR recognize: mimeType={MimeType}", mimeType);
        
        var rawLatex = await ocrProvider.RecognizeLatexAsync(base64Image, mimeType, ct);
        return new OcrResponse { Latex = CleanLatex(rawLatex) };
    }

    private static (string base64Image, string mimeType) ParseDataUrl(OcrRequest request)
    {
        var imageBase64DataUrl = request?.ImageBase64DataUrl;
        if (string.IsNullOrWhiteSpace(imageBase64DataUrl))
            throw new CustomException(NumCalcErrorCode.EmptyData, "");
        
        var parts = imageBase64DataUrl.Split(',');
        if (parts.Length != 2)
            throw new CustomException(NumCalcErrorCode.InvalidData, "");
        
        var header = parts[0];
        var base64Image = parts[1];
        var mimeType = header.Replace("data:", "").Replace(";base64", "");

        if (string.IsNullOrWhiteSpace(base64Image) || string.IsNullOrWhiteSpace(mimeType))
            throw new CustomException(NumCalcErrorCode.InvalidData, "");

        return (base64Image, mimeType);
    }
    
    private static string CleanLatex(string text) =>
        text.Replace("```latex", "")
            .Replace("```", "")
            .Replace("$", "")
            .Replace("\n", "")
            .Trim();
}