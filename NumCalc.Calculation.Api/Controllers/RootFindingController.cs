using Microsoft.AspNetCore.Mvc;
using NumCalc.Shared.Calculation.Requests;
using NumCalc.Shared.Calculation.Responses;
using IRootFinding = NumCalc.Calculation.Api.Services.Interfaces.IRootFinding;

namespace NumCalc.Calculation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RootFindingController(IRootFinding rootFinding) : ControllerBase
{
    [HttpPost("dichotomy")]
    [ProducesResponseType(typeof(RootFindingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult CalculateDichotomy([FromBody] RootFindingRequest request)
    {
        var response = rootFinding.CalculateDichotomy(request);
        return Ok(response);
    }
    
    [HttpPost("newton")]
    public IActionResult CalculateNewton([FromBody] RootFindingRequest request)
    {
        // TODO: add newton method
        return Ok();
    }
}
