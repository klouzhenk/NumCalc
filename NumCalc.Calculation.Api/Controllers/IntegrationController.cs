using Microsoft.AspNetCore.Mvc;
using NumCalc.Calculation.Api.Services.Interfaces;
using NumCalc.Shared.Integration.Requests;
using NumCalc.Shared.Integration.Responses;

namespace NumCalc.Calculation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class IntegrationController(IIntegrationService integrationService) : ControllerBase
{
    [HttpPost("rectangle")]
    [ProducesResponseType(typeof(IntegrationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult SolveRectangle([FromBody] IntegrationRequest request)
    {
        var response = integrationService.SolveRectangle(request);
        return Ok(response);
    }

    [HttpPost("trapezoid")]
    [ProducesResponseType(typeof(IntegrationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult SolveTrapezoid([FromBody] IntegrationRequest request)
    {
        var response = integrationService.SolveTrapezoid(request);
        return Ok(response);
    }

    [HttpPost("simpson")]
    [ProducesResponseType(typeof(IntegrationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult SolveSimpson([FromBody] IntegrationRequest request)
    {
        var response = integrationService.SolveSimpson(request);
        return Ok(response);
    }
}
