using System.Collections;
using CSnakes.Runtime;
using CSnakes.Runtime.Python;
using Microsoft.AspNetCore.Mvc;
using NumCalc.Shared.Calculation.Requests;
using NumCalc.Shared.Calculation.Responses;
using NumCalc.Shared.Common;

namespace NumCalc.Calculation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RootFindingController(IRootFinding rootFindingService, IFunctionBuilding functionBuilding) : ControllerBase
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

        var pyPointsList = functionBuilding.RootFindingPoints(request.FunctionExpression ?? "", request.StartRange,
            request.EndRange, 100);
        
        var response = new RootFindingResponse
        {
            Method = rawResult.GetAttr("method").As<string>(),
            Root = rawResult.GetAttr("root").As<double>(),
            Iterations = rawResult.GetAttr("iterations").As<int>(),
            IsSuccess = rawResult.GetAttr("is_success").As<bool>(),
            ErrorMessage = errorMsgAttr.IsNone() ? null : errorMsgAttr.As<string>(),
            ChartData = pyPointsList.Select(p => new Point(p.Item1, p.Item2)).ToList()
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
