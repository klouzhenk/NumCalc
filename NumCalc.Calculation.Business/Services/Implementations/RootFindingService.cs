using System.Diagnostics;
using CSnakes.Runtime;
using Microsoft.Extensions.Logging;
using NumCalc.Calculation.Business.Entities.RootFinding;
using NumCalc.Calculation.Business.Exceptions;
using NumCalc.Calculation.Business.Services.Interfaces;
using NumCalc.Calculation.Business.Utils;
using NumCalc.Shared.DTOs.RootFinding;
using NumCalc.Shared.Enums;
using NumCalc.Shared.Enums.RootFinding;
using NumCalc.Shared.RootFinding.Requests;
using NumCalc.Shared.RootFinding.Responses;

namespace NumCalc.Calculation.Business.Services.Implementations;

public class RootFindingService(IPythonEnvironment env, ILogger<RootFindingService> logger) : IRootFindingService
{
    public RootFindingResponse CalculateDichotomy(RootFindingRequest request)
    {
        ValidateRequest(request);

        logger.LogInformation("Dichotomy: f={Expression}, [{Start}, {End}], err={Error}",
            request.FunctionExpression, request.StartRange, request.EndRange, request.Tolerance);

        var rootSolver = env.RootFinding();

        var jsonEnvelope = rootSolver.SolveDichotomy(
            request.FunctionExpression,
            request.StartRange,
            request.EndRange,
            request.Tolerance
        );

        var rootData = jsonEnvelope.UnwrapOrThrow<RootFindingData>();

        logger.LogInformation("Dichotomy completed: root={Root}, iterations={Iterations}",
            rootData?.Root, rootData?.Iterations);

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
        ValidateRequest(request);

        logger.LogInformation("Newton: f={Expression}, x0={Start}, err={Error}",
            request.FunctionExpression, request.StartRange, request.Tolerance);

        var rootSolver = env.RootFinding();

        var jsonEnvelope = rootSolver.SolveNewton(
            request.FunctionExpression,
            request.StartRange,
            request.Tolerance
        );

        var rootData = jsonEnvelope.UnwrapOrThrow<RootFindingData>();

        logger.LogInformation("Newton completed: root={Root}, iterations={Iterations}",
            rootData?.Root, rootData?.Iterations);

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
        ValidateRequest(request);

        logger.LogInformation("SimpleIterations: f={Expression}, [{Start}, {End}], err={Error}",
            request.FunctionExpression, request.StartRange, request.EndRange, request.Tolerance);

        var rootSolver = env.RootFinding();

        var jsonEnvelope = rootSolver.SolveSimpleIterations(
            request.FunctionExpression,
            request.StartRange,
            request.EndRange,
            request.Tolerance
        );

        var rootData = jsonEnvelope.UnwrapOrThrow<RootFindingData>();

        logger.LogInformation("SimpleIterations completed: root={Root}, iterations={Iterations}",
            rootData?.Root, rootData?.Iterations);

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
        ValidateRequest(request);

        logger.LogInformation("Secant: f={Expression}, [{Start}, {End}], err={Error}",
            request.FunctionExpression, request.StartRange, request.EndRange, request.Tolerance);

        var rootSolver = env.RootFinding();

        var jsonEnvelope = rootSolver.SolveSecant(
            request.FunctionExpression,
            request.StartRange,
            request.EndRange,
            request.Tolerance
        );

        var rootData = jsonEnvelope.UnwrapOrThrow<RootFindingData>();

        logger.LogInformation("Secant completed: root={Root}, iterations={Iterations}",
            rootData?.Root, rootData?.Iterations);

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
        ValidateRequest(request);

        logger.LogInformation("Combined: f={Expression}, [{Start}, {End}], err={Error}",
            request.FunctionExpression, request.StartRange, request.EndRange, request.Tolerance);

        var rootSolver = env.RootFinding();

        var jsonEnvelope = rootSolver.SolveCombined(
            request.FunctionExpression,
            request.StartRange,
            request.EndRange,
            request.Tolerance
        );

        var rootData = jsonEnvelope.UnwrapOrThrow<RootFindingData>();

        logger.LogInformation("Combined completed: root={Root}, iterations={Iterations}",
            rootData?.Root, rootData?.Iterations);

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
        ValidateComparisonRequest(request);

        logger.LogInformation("Compare: f={Expression}, methods={Methods}",
            request.FunctionExpression, string.Join(", ", request.Methods!));

        var rootSolver = env.RootFinding();
        var response = new RootFindingComparisonResponse();
        var stopwatch = new Stopwatch();

        foreach (var rootFindingMethod in request?.Methods ?? [])
        {
            var comparisonResultItem = new BenchmarkResultDto() { Method = rootFindingMethod,};

            try
            {
                stopwatch.Restart();

                var jsonEnvelope = rootFindingMethod switch
                {
                    RootFindingMethod.Dichotomy => rootSolver.SolveDichotomy(request.FunctionExpression, request.StartRange, request.EndRange, request.Tolerance),
                    RootFindingMethod.SimpleIterations => rootSolver.SolveSimpleIterations(request.FunctionExpression, request.StartRange, request.EndRange, request.Tolerance),
                    RootFindingMethod.Newton => rootSolver.SolveNewton(request.FunctionExpression, request.StartRange, request.Tolerance),
                    RootFindingMethod.Secant => rootSolver.SolveSecant(request.FunctionExpression, request.StartRange, request.EndRange, request.Tolerance),
                    RootFindingMethod.Combined => rootSolver.SolveCombined(request.FunctionExpression, request.StartRange, request.EndRange, request.Tolerance),
                    _ => throw new CustomException(NumCalcErrorCode.NotImplemented, string.Empty)
                };
                stopwatch.Stop();

                var rootData = jsonEnvelope.UnwrapOrThrow<RootFindingData?>();

                comparisonResultItem.ExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds;
                comparisonResultItem.Root = rootData?.Root;
                comparisonResultItem.Iterations = rootData?.Iterations ?? 0;

                logger.LogInformation("Compare/{Method}: root={Root}, iterations={Iterations}, elapsed={ElapsedMs}ms",
                    rootFindingMethod, rootData?.Root, rootData?.Iterations, stopwatch.Elapsed.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                comparisonResultItem.ExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds;
                logger.LogWarning(ex, "Compare/{Method} failed after {ElapsedMs}ms", rootFindingMethod, stopwatch.Elapsed.TotalMilliseconds);
            }

            response.Results.Add(comparisonResultItem);
        }

        var bestMethod = response.Results
            .Where(r => r.Root.HasValue)
            .OrderBy(r => r.ExecutionTimeMs)
            .ThenBy(r => r.Iterations)
            .FirstOrDefault();
        response.BestMethod = bestMethod?.Method ?? request?.Methods?.FirstOrDefault();

        logger.LogInformation("Compare completed: best method={BestMethod}", response.BestMethod);

        return response;
    }

    private static void ValidateRequest(RootFindingRequest request, bool isNewton = false)
    {
        if (request is null)
            throw new CustomException(NumCalcErrorCode.EmptyData, "The request object cannot be null");
        
        if (string.IsNullOrWhiteSpace(request.FunctionExpression))
            throw new CustomException(NumCalcErrorCode.SyntaxError, "The entered function expression is empty");

        if (!double.IsFinite(request.StartRange))
            throw new CustomException(NumCalcErrorCode.InvalidData, "Start range must be a finite number");
        
        if (!isNewton && !double.IsFinite(request.EndRange))
            throw new CustomException(NumCalcErrorCode.InvalidData, "End range must be a finite number");

        if (!isNewton && request.StartRange >= request.EndRange)
            throw new CustomException(NumCalcErrorCode.RangeInvalid, "Start range must be less than end range");
    }
    
    private static void ValidateComparisonRequest(RootFindingComparisonRequest request)
    {
        if (request is null)
            throw new CustomException(NumCalcErrorCode.EmptyData, "The request object cannot be null");
        
        if (request.Methods is null || !request.Methods.Any())
            throw new CustomException(NumCalcErrorCode.EmptyData, "At least one method must be selected");
        
        if (string.IsNullOrWhiteSpace(request.FunctionExpression))
            throw new CustomException(NumCalcErrorCode.SyntaxError, "The entered function expression is empty");

        if (!double.IsFinite(request.StartRange))
            throw new CustomException(NumCalcErrorCode.InvalidData, "Start range must be a finite number");

        var hasNonNewton = request.Methods.Any(method => method is not RootFindingMethod.Newton);
        
        if (hasNonNewton && !double.IsFinite(request.EndRange))
            throw new CustomException(NumCalcErrorCode.InvalidData, "End range must be a finite number");

        if (hasNonNewton && request.StartRange >= request.EndRange)
            throw new CustomException(NumCalcErrorCode.RangeInvalid, "Start range must be less than end range");
    }
}