using System.Diagnostics;
using CSnakes.Runtime;
using Microsoft.Extensions.Logging;
using NumCalc.Calculation.Api.Entities.RootFinding;
using NumCalc.Calculation.Api.Exceptions;
using NumCalc.Calculation.Api.Services.Interfaces;
using NumCalc.Calculation.Api.Utils;
using NumCalc.Shared.DTOs.RootFinding;
using NumCalc.Shared.Enums;
using NumCalc.Shared.Enums.RootFinding;
using NumCalc.Shared.RootFinding.Requests;
using NumCalc.Shared.RootFinding.Responses;

namespace NumCalc.Calculation.Api.Services.Implementations;

public class RootFindingService(IPythonEnvironment env, ILogger<RootFindingService> logger) : IRootFindingService
{
    public RootFindingResponse CalculateDichotomy(RootFindingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FunctionExpression))
            throw new CustomException(NumCalcErrorCode.SyntaxError, "The entered function expression is empty");

        logger.LogInformation("Dichotomy: f={Expression}, [{Start}, {End}], err={Error}",
            request.FunctionExpression, request.StartRange, request.EndRange, request.Error);

        var rootSolver = env.RootFinding();

        var jsonEnvelope = rootSolver.SolveDichotomy(
            request.FunctionExpression,
            request.StartRange,
            request.EndRange,
            request.Error
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
        if (string.IsNullOrWhiteSpace(request.FunctionExpression))
            throw new CustomException(NumCalcErrorCode.SyntaxError, "The entered function expression is empty");

        logger.LogInformation("Newton: f={Expression}, x0={Start}, err={Error}",
            request.FunctionExpression, request.StartRange, request.Error);

        var rootSolver = env.RootFinding();

        var jsonEnvelope = rootSolver.SolveNewton(
            request.FunctionExpression,
            request.StartRange,
            request.Error
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
        if (string.IsNullOrWhiteSpace(request.FunctionExpression))
            throw new CustomException(NumCalcErrorCode.SyntaxError, "The entered function expression is empty");

        logger.LogInformation("SimpleIterations: f={Expression}, [{Start}, {End}], err={Error}",
            request.FunctionExpression, request.StartRange, request.EndRange, request.Error);

        var rootSolver = env.RootFinding();

        var jsonEnvelope = rootSolver.SolveSimpleIterations(
            request.FunctionExpression,
            request.StartRange,
            request.EndRange,
            request.Error
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
        if (string.IsNullOrWhiteSpace(request.FunctionExpression))
            throw new CustomException(NumCalcErrorCode.SyntaxError, "The entered function expression is empty");

        logger.LogInformation("Secant: f={Expression}, [{Start}, {End}], err={Error}",
            request.FunctionExpression, request.StartRange, request.EndRange, request.Error);

        var rootSolver = env.RootFinding();

        var jsonEnvelope = rootSolver.SolveSecant(
            request.FunctionExpression,
            request.StartRange,
            request.EndRange,
            request.Error
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
        if (string.IsNullOrWhiteSpace(request.FunctionExpression))
            throw new CustomException(NumCalcErrorCode.SyntaxError, "The entered function expression is empty");

        logger.LogInformation("Combined: f={Expression}, [{Start}, {End}], err={Error}",
            request.FunctionExpression, request.StartRange, request.EndRange, request.Error);

        var rootSolver = env.RootFinding();

        var jsonEnvelope = rootSolver.SolveCombined(
            request.FunctionExpression,
            request.StartRange,
            request.EndRange,
            request.Error
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
        if (string.IsNullOrWhiteSpace(request.FunctionExpression))
            throw new CustomException(NumCalcErrorCode.SyntaxError, "The entered function expression is empty");

        logger.LogInformation("Compare: f={Expression}, methods={Methods}",
            request.FunctionExpression, string.Join(", ", request?.Methods ?? []));

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
}