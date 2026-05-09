using NumCalc.Shared.OCR.Requests;
using NumCalc.Shared.OCR.Responses;

namespace NumCalc.Calculation.Business.Services.Interfaces;

public interface IOcrService
{
    Task<OcrResponse> RecognizeAsync(OcrRequest request, CancellationToken ct = default);
}