using Microsoft.AspNetCore.Mvc;
using NumCalc.Calculation.Api.Services.Interfaces;
using NumCalc.Shared.Interpolation.Requests;
using NumCalc.Shared.Interpolation.Responses;

namespace NumCalc.Calculation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class InterpolationController(IInterpolationService interpolationService) : ControllerBase
{
    [HttpPost("newton")]
    [ProducesResponseType(typeof(InterpolationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult SolveNewton([FromBody] InterpolationRequest request)
    {
        var response = interpolationService.SolveNewton(request);
        return Ok(response);
    }

    [HttpPost("lagrange")]
    [ProducesResponseType(typeof(InterpolationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult SolveLagrange([FromBody] InterpolationRequest request)
    {
        var response = interpolationService.SolveLagrange(request);
        return Ok(response);
    }

    [HttpPost("spline")]
    [ProducesResponseType(typeof(InterpolationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult SolveSpline([FromBody] InterpolationRequest request)
    {
        var response = interpolationService.SolveSpline(request);
        return Ok(response);
    }
}
