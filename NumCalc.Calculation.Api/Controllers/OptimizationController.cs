using Microsoft.AspNetCore.Mvc;
using NumCalc.Calculation.Api.Services.Interfaces;
using NumCalc.Shared.Optimization.Requests;
using NumCalc.Shared.Optimization.Responses;

namespace NumCalc.Calculation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OptimizationController(IOptimizationService optimizationService) : ControllerBase
{
    [HttpPost("uniform-search")]
    [ProducesResponseType(typeof(OptimizationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult SolveUniformSearch([FromBody] OptimizationRequest request)
    {
        var response = optimizationService.SolveUniformSearch(request);
        return Ok(response);
    }

    [HttpPost("golden-section")]
    [ProducesResponseType(typeof(OptimizationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult SolveGoldenSection([FromBody] OptimizationRequest request)
    {
        var response = optimizationService.SolveGoldenSection(request);
        return Ok(response);
    }

    [HttpPost("gradient-descent")]
    [ProducesResponseType(typeof(OptimizationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult SolveGradientDescent([FromBody] GradientDescentRequest request)
    {
        var response = optimizationService.SolveGradientDescent(request);
        return Ok(response);
    }
}
