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
public class SavedFileController(ISavedFileService savedFileService) : AuthorizedControllerBase
{
    /// <summary>Returns metadata for all saved files of the current user.</summary>
    /// <returns>List of saved file metadata.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<SavedFileMetadataDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllMeta()
    {
        var result = await savedFileService.GetAllMetaAsync(CurrentUserId);
        return Ok(result);
    }

    /// <summary>Returns metadata for the last N saved files of the current user.</summary>
    /// <param name="count">Number of records to return (default 5).</param>
    [HttpGet("last")]
    [ProducesResponseType(typeof(List<SavedFileMetadataDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLastMeta([FromQuery] int count = 5)
    {
        var result = await savedFileService.GetLastMetaAsync(CurrentUserId, count);
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
        var fileData = await savedFileService.DownloadAsync(CurrentUserId, id);
        return File(fileData, "application/octet-stream");
    }

    /// <summary>Saves an exported file. Enforces a maximum of 10 files per user.</summary>
    /// <param name="request">File data and metadata to save.</param>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Save([FromBody] SaveFileRequest request)
    {
        await savedFileService.SaveAsync(CurrentUserId, request);
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
        await savedFileService.DeleteAsync(CurrentUserId, id);
        return NoContent();
    }
}
