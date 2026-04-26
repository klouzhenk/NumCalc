using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NumCalc.User.Application.DTOs;
using NumCalc.User.Application.Interfaces.Services;
using NumCalc.User.Domain.Enums;

namespace NumCalc.User.API.Controllers;

/// <summary>Manages saved calculation inputs for the authenticated user.</summary>
[ApiController]
[Authorize]
[Route("api/saved-inputs")]
[Produces("application/json")]
public class SavedInputController(ISavedInputService savedInputService) : AuthorizedControllerBase
{
    /// <summary>Returns saved inputs for the current user, optionally filtered by calculation type.</summary>
    /// <param name="type">Optional calculation type filter.</param>
    /// <returns>List of saved inputs.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<SavedInputDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll([FromQuery] CalculationType? type = null)
    {
        var result = type.HasValue
            ? await savedInputService.GetByTypeAsync(CurrentUserId, type.Value)
            : await savedInputService.GetAllAsync(CurrentUserId);
        return Ok(result);
    }

    /// <summary>Saves a new calculation input.</summary>
    /// <param name="request">Input data to save.</param>
    /// <returns>The created saved input.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(SavedInputDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateSavedInputRequest request)
    {
        var result = await savedInputService.CreateAsync(CurrentUserId, request);
        return CreatedAtAction(nameof(GetAll), result);
    }

    /// <summary>Deletes a saved input by ID.</summary>
    /// <param name="id">The ID of the saved input to delete.</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await savedInputService.DeleteAsync(CurrentUserId, id);
        return NoContent();
    }
}
