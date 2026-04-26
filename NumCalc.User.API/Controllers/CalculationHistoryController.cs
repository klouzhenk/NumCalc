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
public class CalculationHistoryController(ICalculationHistoryService calculationHistoryService) : AuthorizedControllerBase
{
    /// <summary>Returns all calculation history records for the current user.</summary>
    /// <returns>List of calculation history records.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<CalculationHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCalculationHistory()
    {
        var result = await calculationHistoryService.GetCalculationHistoryAsync(CurrentUserId);
        return Ok(result);
    }

    /// <summary>Saves a new calculation history record for the current user.</summary>
    /// <param name="request">Calculation record data to save.</param>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SaveRecord([FromBody] SaveCalculationRecordRequest request)
    {
        await calculationHistoryService.SaveAsync(CurrentUserId, request);
        return NoContent();
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
        await calculationHistoryService.DeleteAsync(CurrentUserId, id);
        return NoContent();
    }
}
