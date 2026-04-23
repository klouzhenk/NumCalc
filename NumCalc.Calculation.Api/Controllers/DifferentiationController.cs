using Microsoft.AspNetCore.Mvc;
using NumCalc.Calculation.Api.Services.Interfaces;
using NumCalc.Shared.Differentiation.Requests;
using NumCalc.Shared.Differentiation.Responses;

namespace NumCalc.Calculation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DifferentiationController(IDifferentiationService differentiationService) : ControllerBase
{
    [HttpPost("forward")]
    [ProducesResponseType(typeof(DifferentiationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult SolveForward([FromBody] DifferentiationRequest request)
        => Ok(differentiationService.SolveForward(request));

    [HttpPost("backward")]
    [ProducesResponseType(typeof(DifferentiationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult SolveBackward([FromBody] DifferentiationRequest request)
        => Ok(differentiationService.SolveBackward(request));

    [HttpPost("central")]
    [ProducesResponseType(typeof(DifferentiationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult SolveCentral([FromBody] DifferentiationRequest request)
        => Ok(differentiationService.SolveCentral(request));

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
