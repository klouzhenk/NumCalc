using CSnakes.Runtime;
using Microsoft.Extensions.Logging;
using NumCalc.Calculation.Api.Entities.Optimization;
using NumCalc.Calculation.Api.Services.Interfaces;
using NumCalc.Calculation.Api.Utils;
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
            request.Points
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
            request.Tolerance
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
            request.MaxIterations
        );

        var result = jsonEnvelope.UnwrapOrThrow<OptimizationData>();
        stopwatch.Stop();

        logger.LogInformation("GradientDescent completed: min={Value}, elapsed={ElapsedMs}ms",
            result.MinimumValue, stopwatch.Elapsed.TotalMilliseconds);

        return MapToResponse(result, stopwatch.Elapsed.TotalMilliseconds);
    }

    private static OptimizationResponse MapToResponse(OptimizationData data, double executionTimeMs) =>
        new()
        {
            MinimumValue = data.MinimumValue,
            ArgMinX = data.ArgMinX,
            ArgMinPoint = data.ArgMinPoint,
            ChartData = data.ChartPoints,
            SolutionSteps = data.SolutionSteps,
            ExecutionTimeMs = executionTimeMs
        };
}
