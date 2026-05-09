namespace NumCalc.Calculation.Business.Services.Interfaces;

public interface IOcrProvider
{
    Task<string> RecognizeLatexAsync(string base64Image, string mimeType, CancellationToken ct = default);
}