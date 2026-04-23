using Microsoft.AspNetCore.Mvc;
using NumCalc.Calculation.Api.Services.Interfaces;
using NumCalc.Shared.ODE.Requests;
using NumCalc.Shared.ODE.Responses;

namespace NumCalc.Calculation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OdeController(IOdeService odeService) : ControllerBase
{
    [HttpPost("euler")]
    [ProducesResponseType(typeof(OdeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult SolveEuler([FromBody] OdeRequest request)
    {
        var response = odeService.SolveEuler(request);
        return Ok(response);
    }

    [HttpPost("euler-improved")]
    [ProducesResponseType(typeof(OdeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult SolveEulerImproved([FromBody] OdeRequest request)
    {
        var response = odeService.SolveEulerImproved(request);
        return Ok(response);
    }

    [HttpPost("runge-kutta-2")]
    [ProducesResponseType(typeof(OdeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult SolveRungeKutta2([FromBody] OdeRequest request)
    {
        var response = odeService.SolveRungeKutta2(request);
        return Ok(response);
    }

    [HttpPost("runge-kutta-4")]
    [ProducesResponseType(typeof(OdeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult SolveRungeKutta4([FromBody] OdeRequest request)
    {
        var response = odeService.SolveRungeKutta4(request);
        return Ok(response);
    }

    [HttpPost("picard")]
    [ProducesResponseType(typeof(OdeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult SolvePicard([FromBody] OdeRequest request)
    {
        var response = odeService.SolvePicard(request);
        return Ok(response);
    }

    [HttpPost("comparison")]
    [ProducesResponseType(typeof(OdeComparisonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult Compare([FromBody] OdeComparisonRequest request)
    {
        var response = odeService.Compare(request);
        return Ok(response);
    }
}
