using Microsoft.AspNetCore.Mvc;
using NumCalc.Calculation.Business.Services.Interfaces;
using NumCalc.Shared.OCR.Requests;
using NumCalc.Shared.OCR.Responses;

namespace NumCalc.Calculation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OcrController(IOcrService ocrService) : ControllerBase
{
    [HttpPost("recognize")]
    [ProducesResponseType(typeof(OcrResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Recognize([FromBody] OcrRequest request, CancellationToken ct)
    {
        var response = await ocrService.RecognizeAsync(request, ct);
        return Ok(response);
    }
}