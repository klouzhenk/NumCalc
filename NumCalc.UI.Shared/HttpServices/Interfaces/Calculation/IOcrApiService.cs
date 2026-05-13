using NumCalc.Shared.OCR.Requests;
using NumCalc.Shared.OCR.Responses;

namespace NumCalc.UI.Shared.HttpServices.Interfaces.Calculation;

public interface IOcrApiService
{
    Task<OcrResponse?> RecognizeExpressionAsync(OcrRequest request);
}