using Microsoft.AspNetCore.Mvc;
using NumCalc.Calculation.Api.Services.Interfaces;
using NumCalc.Shared.EquationsSystems.Requests;
using NumCalc.Shared.EquationsSystems.Responses;

namespace NumCalc.Calculation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EquationsSystemsController(IEquationsSystemService equationsSystemService) : ControllerBase
{
    [HttpPost("cramer")]
    [ProducesResponseType(typeof(SystemSolvingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult SolveCramer([FromBody] SystemSolvingRequest request)
    {
        var response = equationsSystemService.SolveCramer(request);
        return Ok(response);
    }

    [HttpPost("gaussian")]
    [ProducesResponseType(typeof(SystemSolvingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult SolveGaussian([FromBody] SystemSolvingRequest request)
    {
        var response = equationsSystemService.SolveGaussian(request);
        return Ok(response);
    }
}