using CSnakes.Runtime;
using Microsoft.AspNetCore.Mvc;
using NumCalc.Shared.Calculation.Requests;
using NumCalc.Shared.Calculation.Responses;

namespace NumCalc.Calculation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RootFindingController(IRootFinding rootFindingService) : ControllerBase
{
    [HttpPost("dichotomy")]
    public IActionResult CalculateDichotomy([FromBody] RootFindingRequest request)
    {
        using var rawResult = rootFindingService.SolveDichotomy(
            request.FunctionExpression ?? string.Empty, 
            request.StartRange, 
            request.EndRange, 
            request.Error
        );
        
        var errorMsgAttr = rawResult.GetAttr("error_message");
        
        var response = new RootFindingResponse
        {
            Method = rawResult.GetAttr("method").As<string>(),
            Root = rawResult.GetAttr("root").As<double>(),
            Iterations = rawResult.GetAttr("iterations").As<int>(),
            IsSuccess = rawResult.GetAttr("is_success").As<bool>(),
            ErrorMessage = errorMsgAttr.IsNone() ? null : errorMsgAttr.As<string>()
        };
    
        return Ok(response);
    }
    
    [HttpPost("newton")]
    public IActionResult CalculateNewton([FromBody] RootFindingRequest request)
    {
        using var rawResult = rootFindingService.SolveDichotomy(
            request.FunctionExpression ?? string.Empty, 
            request.StartRange, 
            request.EndRange, 
            request.Error
        );
        
        var errorMsgAttr = rawResult.GetAttr("error_message");
        
        var response = new RootFindingResponse
        {
            Method = rawResult.GetAttr("method").As<string>(),
            Root = rawResult.GetAttr("root").As<double>(),
            Iterations = rawResult.GetAttr("iterations").As<int>(),
            IsSuccess = rawResult.GetAttr("is_success").As<bool>(),
            ErrorMessage = errorMsgAttr.IsNone() ? null : errorMsgAttr.As<string>()
        };
    
        return Ok(response);
    }
}
