using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NumCalc.User.Application.DTOs;
using NumCalc.User.Application.Interfaces.Services;

namespace NumCalc.User.API.Controllers;

/// <summary>Manages calculation history for the authenticated user.</summary>
[ApiController]
[Authorize]
[Route("api/calculation-history")]
[Produces("application/json")]
public class CalculationHistoryController(ICalculationHistoryService calculationHistoryService) : ControllerBase
{
    /// <summary>Returns all calculation history records for the current user.</summary>
    /// <returns>List of calculation history records.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<CalculationHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCalculationHistory()
    {
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);
        var result = await calculationHistoryService.GetCalculationHistoryAsync(userId);
        return Ok(result);
    }

    /// <summary>Deletes a specific calculation history record.</summary>
    /// <param name="id">The ID of the record to delete.</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteCalculationHistoryRecord(Guid id)
    {
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);
        await calculationHistoryService.DeleteAsync(userId, id);
        return NoContent();
    }
}
