using System.Text.Json;
using CSnakes.Runtime;
using NumCalc.Calculation.Api.Entities;
using NumCalc.Calculation.Api.Exceptions;
using NumCalc.Calculation.Api.Services.Interfaces;
using NumCalc.Shared.Calculation.Requests;
using NumCalc.Shared.Calculation.Responses;
using NumCalc.Shared.Common;
using NumCalc.Shared.Enums;

namespace NumCalc.Calculation.Api.Services.Implementations;

public class RootFindingService(IPythonEnvironment env) : IRootFindingService
{
    public RootFindingResponse CalculateDichotomy(RootFindingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FunctionExpression))
            throw new CustomException(NumCalcErrorCode.SyntaxError, "The entered function expression is empty");
     
        var rootSolver = env.RootFinding();
        var functionBuilding = env.FunctionBuilder();
        
        var jsonEnvelope = rootSolver.SolveDichotomy(
            request.FunctionExpression, 
            request.StartRange, 
            request.EndRange, 
            request.Error
        );
        
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true 
        };
        var envelope = JsonSerializer.Deserialize<PythonResponse<RootFindingData>>(jsonEnvelope, options);
        
        // TODO: add some overall response class and add c#-wrapper
        if (envelope?.Failure is not null)
        {
            var codeStr = envelope.Failure.Code;
            var msg = envelope.Failure.Message;

            var errorCode = Enum.TryParse<NumCalcErrorCode>(codeStr, true, out var code) 
                ? code 
                : NumCalcErrorCode.UnknownError;

            throw new CustomException(errorCode, msg);
        }

        if (envelope?.Success is null)
            throw new CustomException(NumCalcErrorCode.UnknownError, "The response was not successful");
        
        var pyPointsList = functionBuilding.RootFindingPoints(request.FunctionExpression ?? "", request.StartRange,
            request.EndRange, 100);
        
        var response = new RootFindingResponse
        {
            Root = envelope?.Success?.Root,
            Iterations = envelope?.Success?.Iterations ?? 0,
            ChartData = pyPointsList.Select(p => new Point(p.Item1, p.Item2))
        };

        return response;
    }
}