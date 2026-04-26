using Microsoft.AspNetCore.Mvc;
using NumCalc.Shared.User.Requests;
using NumCalc.Shared.User.Responses;
using NumCalc.User.Application.Interfaces.Services;

namespace NumCalc.User.API.Controllers;

/// <summary>Handles user registration and authentication.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>Registers a new user account.</summary>
    /// <param name="request">Username and password for the new account.</param>
    /// <returns>JWT token and username on success.</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var response = await authService.RegisterAsync(request);
        return Ok(response);
    }

    /// <summary>Authenticates a user and returns a JWT token.</summary>
    /// <param name="request">Username and password credentials.</param>
    /// <returns>JWT token and username on success.</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await authService.LoginAsync(request);
        return Ok(response);
    }
}