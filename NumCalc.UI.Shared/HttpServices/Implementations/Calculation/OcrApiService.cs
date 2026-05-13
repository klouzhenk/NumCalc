using NumCalc.Shared.OCR.Requests;
using NumCalc.Shared.OCR.Responses;
using NumCalc.UI.Shared.HttpServices.Interfaces.Calculation;

namespace NumCalc.UI.Shared.HttpServices.Implementations.Calculation;

public class OcrApiService(HttpClient httpClient) : BaseApiService(httpClient), IOcrApiService
{
    protected override string ApiControllerName => "api/ocr";
    
    public async Task<OcrResponse?> RecognizeExpressionAsync(OcrRequest request)
        => await SendPostRequestAsync<OcrResponse>($"{ApiControllerName}/recognize", request);
}