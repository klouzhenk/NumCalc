using CSnakes.Runtime;
using NumCalc.Calculation.Api.Exceptions;
using NumCalc.Shared.Calculation.Requests;
using NumCalc.Shared.Calculation.Responses;
using NumCalc.Shared.Common;
using NumCalc.Shared.Enums;
using IRootFinding = NumCalc.Calculation.Api.Services.Interfaces.IRootFinding;

namespace NumCalc.Calculation.Api.Services.Implementations;

public class RootFinding(CSnakes.Runtime.IRootFinding rootSolver, IFunctionBuilding functionBuilding) : IRootFinding
{
    public RootFindingResponse CalculateDichotomy(RootFindingRequest request)
    {
        if (request is null || request.StartRange > request.EndRange)
            throw new CustomException(NumCalcErrorCode.RangeInvalid, "Range to find root is not valid");
        
        var rawResult = rootSolver.SolveDichotomy(
            request.FunctionExpression ?? string.Empty, 
            request.StartRange, 
            request.EndRange, 
            request.Error
        );

        var pyPointsList = functionBuilding.RootFindingPoints(request.FunctionExpression ?? "", request.StartRange,
            request.EndRange, 100);
        
        var response = new RootFindingResponse
        {
            Method = rawResult.GetAttr("method").As<string>(),
            Root = rawResult.GetAttr("root").As<double>(),
            Iterations = rawResult.GetAttr("iterations").As<int>(),
            ChartData = pyPointsList.Select(p => new Point(p.Item1, p.Item2)).ToList()
        };

        return response;
    }
}