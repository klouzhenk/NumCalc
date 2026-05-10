using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NumCalc.User.Application.DTOs;
using NumCalc.User.Application.Interfaces.Services;

namespace NumCalc.User.API.Controllers;

/// <summary>Manages the authenticated user's account profile.</summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
[Produces("application/json")]
public class UserController(IUserService userService) : AuthorizedControllerBase
{
    /// <summary>Returns the authenticated user's profile.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserProfile()
    {
        var response = await userService.GetCurrentUserAsync(CurrentUserId);
        return Ok(response);
    }

    /// <summary>Updates the authenticated user's profile (username, email, and/or password).</summary>
    /// <remarks>Requires the current password. Only fields provided in the body are updated.</remarks>
    [HttpPatch]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        await userService.UpdateProfileAsync(CurrentUserId, request);
        return NoContent();
    }

    /// <summary>Deletes the authenticated user's account and all associated data.</summary>
    /// <remarks>Requires the current password. Cascades to history, saved inputs, and saved files.</remarks>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest request)
    {
        await userService.DeleteAccountAsync(CurrentUserId, request);
        return NoContent();
    }
}
