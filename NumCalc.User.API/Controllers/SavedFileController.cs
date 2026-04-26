using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NumCalc.User.Application.DTOs;
using NumCalc.User.Application.Interfaces.Services;

namespace NumCalc.User.API.Controllers;

/// <summary>Manages saved exported files for the authenticated user.</summary>
[ApiController]
[Authorize]
[Route("api/saved-files")]
[Produces("application/json")]
public class SavedFileController(ISavedFileService savedFileService) : ControllerBase
{
    /// <summary>Returns metadata for all saved files of the current user.</summary>
    /// <returns>List of saved file metadata.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<SavedFileMetadataDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllMeta()
    {
        // TODO : move the userId extraction to another reusable method
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);
        var result = await savedFileService.GetAllMetaAsync(userId);
        return Ok(result);
    }

    /// <summary>Downloads a saved file by ID.</summary>
    /// <param name="id">The ID of the file to download.</param>
    /// <returns>File bytes as application/octet-stream.</returns>
    [HttpGet("{id}/download")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Download(Guid id)
    {
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);
        var fileData = await savedFileService.DownloadAsync(userId, id);
        return File(fileData, "application/octet-stream");
    }

    /// <summary>Saves an exported file. Enforces a maximum of 10 files per user.</summary>
    /// <param name="request">File data and metadata to save.</param>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Save([FromBody] SaveFileRequest request)
    {
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);
        await savedFileService.SaveAsync(userId, request);
        return NoContent();
    }

    /// <summary>Deletes a saved file by ID.</summary>
    /// <param name="id">The ID of the file to delete.</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid id)
    {
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);
        await savedFileService.DeleteAsync(userId, id);
        return NoContent();
    }
}
