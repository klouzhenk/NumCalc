using CSnakes.Runtime;
using Microsoft.Extensions.Logging;
using NumCalc.Calculation.Api.Entities.Interpolation;
using NumCalc.Calculation.Api.Services.Interfaces;
using NumCalc.Calculation.Api.Utils;
using NumCalc.Shared.Enums.Interpolation;
using NumCalc.Shared.Interpolation.Requests;
using NumCalc.Shared.Interpolation.Responses;

namespace NumCalc.Calculation.Api.Services.Implementations;

public class InterpolationService(IPythonEnvironment env, ILogger<InterpolationService> logger) : IInterpolationService
{
    public InterpolationResponse SolveNewton(InterpolationRequest request)
    {
        logger.LogInformation("Newton interpolation: mode={Mode}, nodes={NodeCount}, queryPoint={QueryPoint}",
            request.Mode, request.XNodes?.Count, request.QueryPoint);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var solver = env.Interpolation();

        var jsonEnvelope = solver.SolveNewton(
            request.XNodes,
            request.Mode == InterpolationInputMode.RawData ? request.YValues : null,
            request.QueryPoint,
            request.Mode == InterpolationInputMode.Function ? request.FunctionExpression : null
        );

        var result = jsonEnvelope.UnwrapOrThrow<InterpolationData>();
        stopwatch.Stop();

        logger.LogInformation("Newton interpolation completed: value={Value}, elapsed={ElapsedMs}ms",
            result.InterpolatedValue, stopwatch.Elapsed.TotalMilliseconds);

        return MapToResponse(result, stopwatch.Elapsed.TotalMilliseconds);
    }

    public InterpolationResponse SolveLagrange(InterpolationRequest request)
    {
        logger.LogInformation("Lagrange interpolation: mode={Mode}, nodes={NodeCount}, queryPoint={QueryPoint}",
            request.Mode, request.XNodes?.Count, request.QueryPoint);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var solver = env.Interpolation();

        var jsonEnvelope = solver.SolveLagrange(
            request.XNodes,
            request.Mode == InterpolationInputMode.RawData ? request.YValues : null,
            request.QueryPoint,
            request.Mode == InterpolationInputMode.Function ? request.FunctionExpression : null
        );

        var result = jsonEnvelope.UnwrapOrThrow<InterpolationData>();
        stopwatch.Stop();

        logger.LogInformation("Lagrange interpolation completed: value={Value}, elapsed={ElapsedMs}ms",
            result.InterpolatedValue, stopwatch.Elapsed.TotalMilliseconds);

        return MapToResponse(result, stopwatch.Elapsed.TotalMilliseconds);
    }

    public InterpolationResponse SolveSpline(InterpolationRequest request)
    {
        logger.LogInformation("Spline interpolation: mode={Mode}, nodes={NodeCount}, queryPoint={QueryPoint}",
            request.Mode, request.XNodes?.Count, request.QueryPoint);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var solver = env.Interpolation();

        var jsonEnvelope = solver.SolveSpline(
            request.XNodes,
            request.Mode == InterpolationInputMode.RawData ? request.YValues : null,
            request.QueryPoint,
            request.Mode == InterpolationInputMode.Function ? request.FunctionExpression : null
        );

        var result = jsonEnvelope.UnwrapOrThrow<InterpolationData>();
        stopwatch.Stop();

        logger.LogInformation("Spline interpolation completed: value={Value}, elapsed={ElapsedMs}ms",
            result.InterpolatedValue, stopwatch.Elapsed.TotalMilliseconds);

        return MapToResponse(result, stopwatch.Elapsed.TotalMilliseconds);
    }

    private static InterpolationResponse MapToResponse(InterpolationData data, double executionTimeMs) =>
        new()
        {
            InterpolatedValue = data.InterpolatedValue,
            PolynomialLatex = data.PolynomialLatex,
            ChartData = data.ChartPoints,
            SolutionSteps = data.SolutionSteps,
            ExecutionTimeMs = executionTimeMs
        };
}
