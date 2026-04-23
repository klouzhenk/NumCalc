using System.Diagnostics;
using CSnakes.Runtime;
using Microsoft.Extensions.Logging;
using NumCalc.Calculation.Api.Entities.Optimization;
using NumCalc.Calculation.Api.Services.Interfaces;
using NumCalc.Calculation.Api.Utils;
using NumCalc.Shared.DTOs.Optimization;
using NumCalc.Shared.Enums.Optimization;
using NumCalc.Shared.Optimization.Requests;
using NumCalc.Shared.Optimization.Responses;

namespace NumCalc.Calculation.Api.Services.Implementations;

public class OptimizationService(IPythonEnvironment env, ILogger<OptimizationService> logger) : IOptimizationService
{
    public OptimizationResponse SolveUniformSearch(OptimizationRequest request)
    {
        logger.LogInformation("UniformSearch: f={Expression}, [{Lower}, {Upper}], points={Points}",
            request.FunctionExpression, request.LowerBound, request.UpperBound, request.Points);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var solver = env.Optimization();

        var jsonEnvelope = solver.SolveUniformSearch(
            request.FunctionExpression,
            request.LowerBound,
            request.UpperBound,
            request.Points,
            request.Maximize
        );

        var result = jsonEnvelope.UnwrapOrThrow<OptimizationData>();
        stopwatch.Stop();

        logger.LogInformation("UniformSearch completed: min={Value}, argMin={ArgMin}, elapsed={ElapsedMs}ms",
            result.MinimumValue, result.ArgMinX, stopwatch.Elapsed.TotalMilliseconds);

        return MapToResponse(result, stopwatch.Elapsed.TotalMilliseconds);
    }

    public OptimizationResponse SolveGoldenSection(OptimizationRequest request)
    {
        logger.LogInformation("GoldenSection: f={Expression}, [{Lower}, {Upper}], tol={Tolerance}",
            request.FunctionExpression, request.LowerBound, request.UpperBound, request.Tolerance);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var solver = env.Optimization();

        var jsonEnvelope = solver.SolveGoldenSection(
            request.FunctionExpression,
            request.LowerBound,
            request.UpperBound,
            request.Tolerance,
            request.Maximize
        );

        var result = jsonEnvelope.UnwrapOrThrow<OptimizationData>();
        stopwatch.Stop();

        logger.LogInformation("GoldenSection completed: min={Value}, argMin={ArgMin}, elapsed={ElapsedMs}ms",
            result.MinimumValue, result.ArgMinX, stopwatch.Elapsed.TotalMilliseconds);

        return MapToResponse(result, stopwatch.Elapsed.TotalMilliseconds);
    }

    public OptimizationResponse SolveGradientDescent(GradientDescentRequest request)
    {
        logger.LogInformation("GradientDescent: f={Expression}, lr={LearningRate}, tol={Tolerance}, maxIter={MaxIterations}",
            request.FunctionExpression, request.LearningRate, request.Tolerance, request.MaxIterations);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var solver = env.Optimization();

        var jsonEnvelope = solver.SolveGradientDescent(
            request.FunctionExpression,
            request.InitialPoint,
            request.LearningRate,
            request.Tolerance,
            request.MaxIterations,
            request.Maximize
        );

        var result = jsonEnvelope.UnwrapOrThrow<OptimizationData>();
        stopwatch.Stop();

        logger.LogInformation("GradientDescent completed: min={Value}, elapsed={ElapsedMs}ms",
            result.MinimumValue, stopwatch.Elapsed.TotalMilliseconds);

        return MapToResponse(result, stopwatch.Elapsed.TotalMilliseconds);
    }

    public OptimizationComparisonResponse Compare(OptimizationComparisonRequest request)
    {
        logger.LogInformation("Compare: f={Expression}, [{Lower}, {Upper}], methods={Methods}",
            request.FunctionExpression, request.LowerBound, request.UpperBound,
            string.Join(", ", request.Methods ?? []));

        var solver = env.Optimization();
        var response = new OptimizationComparisonResponse();
        var stopwatch = new Stopwatch();

        foreach (var method in request.Methods ?? [])
        {
            var item = new OptimizationBenchmarkResultDto { Method = method };

            try
            {
                stopwatch.Restart();

                var jsonEnvelope = method switch
                {
                    OptimizationComparisonMethod.UniformSearch =>
                        solver.SolveUniformSearch(request.FunctionExpression, request.LowerBound, request.UpperBound, request.Points, request.Maximize),
                    OptimizationComparisonMethod.GoldenSection =>
                        solver.SolveGoldenSection(request.FunctionExpression, request.LowerBound, request.UpperBound, request.Tolerance, request.Maximize),
                    _ => throw new ArgumentOutOfRangeException(nameof(method))
                };

                var data = jsonEnvelope.UnwrapOrThrow<OptimizationData>();
                stopwatch.Stop();

                item.MinimumValue = data.MinimumValue;
                item.ArgMinX = data.ArgMinX;
                item.ExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds;

                logger.LogInformation("Compare/{Method}: min={Value}, argMin={ArgMin}, elapsed={ElapsedMs}ms",
                    method, item.MinimumValue, item.ArgMinX, item.ExecutionTimeMs);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                item.ExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds;
                logger.LogWarning(ex, "Compare/{Method} failed after {ElapsedMs}ms", method, stopwatch.Elapsed.TotalMilliseconds);
            }

            response.Results.Add(item);
        }

        response.BestMethod = response.Results
            .Where(r => r.MinimumValue.HasValue)
            .OrderBy(r => r.ExecutionTimeMs)
            .FirstOrDefault()?.Method;

        logger.LogInformation("Compare completed: best method={BestMethod}", response.BestMethod);

        return response;
    }

    private static OptimizationResponse MapToResponse(OptimizationData data, double executionTimeMs) =>
        new()
        {
            MinimumValue = data.MinimumValue,
            ArgMinX = data.ArgMinX,
            ArgMinPoint = data.ArgMinPoint,
            ChartData = data.ChartPoints,
            PathData = data.PathPoints,
            SolutionSteps = data.SolutionSteps,
            ExecutionTimeMs = executionTimeMs
        };
}
