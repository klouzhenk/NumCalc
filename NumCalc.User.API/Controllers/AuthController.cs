using Microsoft.AspNetCore.Mvc;
using NumCalc.User.Application.DTOs;
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
    
    /// <summary>Initiates a password reset for the given email address.</summary>
    /// <remarks>Always returns 200 to avoid leaking whether the email is registered.</remarks>
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        await authService.RequestPasswordResetAsync(request, ct);
        return Ok();
    }
}