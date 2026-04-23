using Microsoft.AspNetCore.Mvc;
using NumCalc.Calculation.Api.Services.Interfaces;
using NumCalc.Shared.Differentiation.Requests;
using NumCalc.Shared.Differentiation.Responses;
using NumCalc.Shared.Enums.Differentiation;

namespace NumCalc.Calculation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DifferentiationController(IDifferentiationService differentiationService) : ControllerBase
{
    [HttpPost("finite-diff")]
    [ProducesResponseType(typeof(DifferentiationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult SolveFiniteDiff([FromBody] DifferentiationRequest request, [FromQuery] FiniteDiffVariant variant = FiniteDiffVariant.Central)
        => Ok(differentiationService.SolveFiniteDiff(request, variant));

    [HttpPost("lagrange")]
    [ProducesResponseType(typeof(DifferentiationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult SolveLagrange([FromBody] DifferentiationRequest request)
        => Ok(differentiationService.SolveLagrange(request));

    [HttpPost("comparison")]
    [ProducesResponseType(typeof(DifferentiationComparisonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult Compare([FromBody] DifferentiationComparisonRequest request)
        => Ok(differentiationService.Compare(request));
}
