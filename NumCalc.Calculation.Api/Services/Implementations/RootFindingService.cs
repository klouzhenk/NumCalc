using System.Diagnostics;
using CSnakes.Runtime;
using NumCalc.Calculation.Api.Entities;
using NumCalc.Calculation.Api.Exceptions;
using NumCalc.Calculation.Api.Services.Interfaces;
using NumCalc.Calculation.Api.Utils;
using NumCalc.Shared.DTOs.RootFinding;
using NumCalc.Shared.Enums;
using NumCalc.Shared.Enums.RootFinding;
using NumCalc.Shared.RootFinding.Requests;
using NumCalc.Shared.RootFinding.Responses;

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

    public RootFindingResponse CalculateSimpleIterations(RootFindingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FunctionExpression))
            throw new CustomException(NumCalcErrorCode.SyntaxError, "The entered function expression is empty");
        
        var rootSolver = env.RootFinding();
        
        var jsonEnvelope = rootSolver.SolveSimpleIterations(
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

    public RootFindingResponse CalculateSecant(RootFindingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FunctionExpression))
            throw new CustomException(NumCalcErrorCode.SyntaxError, "The entered function expression is empty");
        
        var rootSolver = env.RootFinding();
        
        var jsonEnvelope = rootSolver.SolveSecant(
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
    
    public RootFindingResponse CalculateCombined(RootFindingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FunctionExpression))
            throw new CustomException(NumCalcErrorCode.SyntaxError, "The entered function expression is empty");
        
        var rootSolver = env.RootFinding();
        
        var jsonEnvelope = rootSolver.SolveCombined(
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

    public RootFindingComparisonResponse Compare(RootFindingComparisonRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FunctionExpression))
            throw new CustomException(NumCalcErrorCode.SyntaxError, "The entered function expression is empty");
        
        var rootSolver = env.RootFinding();
        var response = new RootFindingComparisonResponse();
        var stopwatch = new Stopwatch();

        foreach (var rootFindingMethod in request?.Methods ?? [])
        {
            var comparisonResultItem = new BenchmarkResultDto() { Method = rootFindingMethod,};
            
            try
            {
                stopwatch.Restart();

                // TODO : use new method for comparison
                var jsonEnvelope = rootFindingMethod switch
                {
                    RootFindingMethod.Dichotomy => rootSolver.SolveDichotomy(request.FunctionExpression, request.StartRange, request.EndRange, request.Tolerance),
                    RootFindingMethod.SimpleIterations => rootSolver.SolveSimpleIterations(request.FunctionExpression, request.StartRange, request.EndRange, request.Tolerance),
                    RootFindingMethod.Newton => rootSolver.SolveNewton(request.FunctionExpression, request.StartRange, request.Tolerance),
                    RootFindingMethod.Secant => rootSolver.SolveSecant(request.FunctionExpression, request.StartRange, request.EndRange, request.Tolerance),
                    RootFindingMethod.Combined => rootSolver.SolveCombined(request.FunctionExpression, request.StartRange, request.EndRange, request.Tolerance),
                    _ => throw new CustomException(NumCalcErrorCode.NotImplemented, string.Empty)
                };
                var rootData = jsonEnvelope.UnwrapOrThrow<RootFindingData?>();
            
                stopwatch.Stop();
            
                comparisonResultItem.ExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds;
                comparisonResultItem.Root = rootData?.Root;
                comparisonResultItem.Iterations = rootData?.Iterations ?? 0;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                comparisonResultItem.ExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds;
            }
            
            response.Results.Add(comparisonResultItem);
        }
        
        var bestMethod = response.Results
            .Where(r => r.Root.HasValue)
            .OrderBy(r => r.ExecutionTimeMs)
            .ThenBy(r => r.Iterations)
            .FirstOrDefault();
        response.BestMethod = bestMethod?.Method ?? request?.Methods?.FirstOrDefault();

        return response;
    }
}