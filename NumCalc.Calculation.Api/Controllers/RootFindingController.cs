using Microsoft.AspNetCore.Mvc;
using NumCalc.Calculation.Api.Services.Interfaces;
using NumCalc.Shared.Calculation.Requests;
using NumCalc.Shared.Calculation.Responses;

namespace NumCalc.Calculation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RootFindingController(IRootFindingService rootFindingService) : ControllerBase
{
    [HttpPost("dichotomy")]
    [ProducesResponseType(typeof(RootFindingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult CalculateDichotomy([FromBody] RootFindingRequest request)
    {
        var response = rootFindingService.CalculateDichotomy(request);
        return Ok(response);
    }
    
    [HttpPost("newton")]
    [ProducesResponseType(typeof(RootFindingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult CalculateNewton([FromBody] RootFindingRequest request)
    {
        var response = rootFindingService.CalculateNewton(request);
        return Ok(response);
    }
    
    [HttpPost("simple-iterations")]
    [ProducesResponseType(typeof(RootFindingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult CalculateSimpleIterations([FromBody] RootFindingRequest request)
    {
        var response = rootFindingService.CalculateSimpleIterations(request);
        return Ok(response);
    }
    
    [HttpPost("secant")]
    [ProducesResponseType(typeof(RootFindingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult CalculateSecant([FromBody] RootFindingRequest request)
    {
        var response = rootFindingService.CalculateSecant(request);
        return Ok(response);
    }
    
    [HttpPost("combined")]
    [ProducesResponseType(typeof(RootFindingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult CalculateCombined([FromBody] RootFindingRequest request)
    {
        var response = rootFindingService.CalculateCombined(request);
        return Ok(response);
    }
}
