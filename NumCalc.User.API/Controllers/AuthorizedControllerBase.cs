using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace NumCalc.User.API.Controllers;

public abstract class AuthorizedControllerBase : ControllerBase
{
    protected Guid CurrentUserId
    {
        get
        {
            Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);
            return userId;
        }
    }
}
