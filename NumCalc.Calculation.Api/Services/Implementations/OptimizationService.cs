using CSnakes.Runtime;
using NumCalc.Calculation.Api.Entities.Optimization;
using NumCalc.Calculation.Api.Services.Interfaces;
using NumCalc.Calculation.Api.Utils;
using NumCalc.Shared.Optimization.Requests;
using NumCalc.Shared.Optimization.Responses;

namespace NumCalc.Calculation.Api.Services.Implementations;

public class OptimizationService(IPythonEnvironment env) : IOptimizationService
{
    public OptimizationResponse SolveUniformSearch(OptimizationRequest request)
    {
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

        return MapToResponse(result, stopwatch.Elapsed.TotalMilliseconds);
    }

    public OptimizationResponse SolveGoldenSection(OptimizationRequest request)
    {
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

        return MapToResponse(result, stopwatch.Elapsed.TotalMilliseconds);
    }

    public OptimizationResponse SolveGradientDescent(GradientDescentRequest request)
    {
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
