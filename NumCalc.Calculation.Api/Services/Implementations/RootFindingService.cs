using CSnakes.Runtime;
using NumCalc.Calculation.Api.Entities;
using NumCalc.Calculation.Api.Exceptions;
using NumCalc.Calculation.Api.Services.Interfaces;
using NumCalc.Calculation.Api.Utils;
using NumCalc.Shared.Calculation.Requests;
using NumCalc.Shared.Calculation.Responses;
using NumCalc.Shared.Enums;

namespace NumCalc.Calculation.Api.Services.Implementations;

public class RootFindingService(IPythonEnvironment env) : IRootFindingService
{
    public RootFindingResponse CalculateDichotomy(RootFindingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FunctionExpression))
            throw new CustomException(NumCalcErrorCode.SyntaxError, "The entered function expression is empty");
     
        var rootSolver = env.RootFinding();
        
        var jsonEnvelope = rootSolver.SolveDichotomy(
            request.FunctionExpression, 
            request.StartRange, 
            request.EndRange, 
            request.Error
        );
        
        var rootData = jsonEnvelope.UnwrapOrThrow<RootFindingData>();

        return new RootFindingResponse
        {
            Root = rootData?.Root,
            Iterations = rootData?.Iterations ?? 0,
            ChartData = rootData?.ChartPoints,
            SolutionSteps = rootData?.SolutionSteps
        };
    }

    public RootFindingResponse CalculateNewton(RootFindingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FunctionExpression))
            throw new CustomException(NumCalcErrorCode.SyntaxError, "The entered function expression is empty");
        
        var rootSolver = env.RootFinding();
        
        var jsonEnvelope = rootSolver.SolveNewton(
            request.FunctionExpression, 
            request.StartRange, 
            request.Error
        );
        
        var rootData = jsonEnvelope.UnwrapOrThrow<RootFindingData>();
        
        return new RootFindingResponse
        {
            Root = rootData?.Root,
            Iterations = rootData?.Iterations ?? 0,
            ChartData = rootData?.ChartPoints,
            SolutionSteps = rootData?.SolutionSteps
        };
    }
}