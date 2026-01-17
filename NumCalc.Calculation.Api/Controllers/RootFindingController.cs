using Microsoft.AspNetCore.Mvc;
using NumCalc.Calculation.Api.Services.Interfaces;
using NumCalc.Shared.Calculation.Requests;
using NumCalc.Shared.Calculation.Responses;

namespace NumCalc.Calculation.Api.Controllers;

/// <summary>
/// Controller for root-finding operations using various numerical methods.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class RootFindingController(IRootFindingService rootFindingService) : ControllerBase
{
    /// <summary>
    /// Computes the root using the Dichotomy (Bisection) method.
    /// </summary>
    /// <remarks>
    /// <b>Methodology:</b> Divides the interval in half at each step.<br/>
    /// <b>Convergence:</b> Linear. Guaranteed to converge if signs at interval endpoints differ.<br/>
    /// <b>Input:</b> Requires a valid interval [a, b].
    /// </remarks>
    /// <param name="request">Calculation parameters including the interval [A, B].</param>
    /// <returns>Calculation result with steps and graph data.</returns>
    [HttpPost("dichotomy")]
    [ProducesResponseType(typeof(RootFindingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult CalculateDichotomy([FromBody] RootFindingRequest request)
    {
        var response = rootFindingService.CalculateDichotomy(request);
        return Ok(response);
    }
    
    /// <summary>
    /// Computes the root using Newton's method (Tangent method).
    /// </summary>
    /// <remarks>
    /// <b>Methodology:</b> Uses the function's derivative to approximate the root.<br/>
    /// <b>Convergence:</b> Quadratic (very fast near the root).<br/>
    /// <b>Input:</b> Requires an initial guess or an interval (uses endpoints as guesses).
    /// </remarks>
    [HttpPost("newton")]
    [ProducesResponseType(typeof(RootFindingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult CalculateNewton([FromBody] RootFindingRequest request)
    {
        var response = rootFindingService.CalculateNewton(request);
        return Ok(response);
    }
    
    /// <summary>
    /// Computes the root using the Fixed-Point Iteration method (Simple Iterations).
    /// </summary>
    /// <remarks>
    /// <b>Methodology:</b> Transforms f(x)=0 into x=g(x). Requires convergence condition |g'(x)| &lt; 1.<br/>
    /// <b>Convergence:</b> Linear, speed depends on the derivative value.
    /// </remarks>
    [HttpPost("simple-iterations")]
    [ProducesResponseType(typeof(RootFindingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult CalculateSimpleIterations([FromBody] RootFindingRequest request)
    {
        var response = rootFindingService.CalculateSimpleIterations(request);
        return Ok(response);
    }
    
    /// <summary>
    /// Computes the root using the Secant method.
    /// </summary>
    /// <remarks>
    /// <b>Methodology:</b> Similar to Newton's method but uses a finite difference approximation for the derivative.<br/>
    /// <b>Convergence:</b> Superlinear (approx 1.618). Does not require analytical derivative calculation.
    /// </remarks>
    [HttpPost("secant")]
    [ProducesResponseType(typeof(RootFindingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult CalculateSecant([FromBody] RootFindingRequest request)
    {
        var response = rootFindingService.CalculateSecant(request);
        return Ok(response);
    }
    
    /// <summary>
    /// Computes the root using the Combined method (Brent's or Hybrid approach).
    /// </summary>
    /// <remarks>
    /// <b>Methodology:</b> Combines Bisection, Secant, and Inverse Quadratic Interpolation.<br/>
    /// <b>Performance:</b> The most robust and generally fastest method. Guaranteed convergence.
    /// </remarks>
    [HttpPost("combined")]
    [ProducesResponseType(typeof(RootFindingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult CalculateCombined([FromBody] RootFindingRequest request)
    {
        var response = rootFindingService.CalculateCombined(request);
        return Ok(response);
    }
}
