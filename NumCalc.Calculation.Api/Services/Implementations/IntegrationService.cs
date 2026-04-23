using System.Diagnostics;
using CSnakes.Runtime;
using Microsoft.Extensions.Logging;
using NumCalc.Calculation.Api.Entities.Integration;
using NumCalc.Calculation.Api.Services.Interfaces;
using NumCalc.Calculation.Api.Utils;
using NumCalc.Shared.DTOs.Integration;
using NumCalc.Shared.Enums.Integration;
using NumCalc.Shared.Integration.Requests;
using NumCalc.Shared.Integration.Responses;

namespace NumCalc.Calculation.Api.Services.Implementations;

public class IntegrationService(IPythonEnvironment env, ILogger<IntegrationService> logger) : IIntegrationService
{
    public IntegrationResponse SolveRectangle(IntegrationRequest request)
    {
        logger.LogInformation("Rectangle: f={Expression}, [{Lower}, {Upper}], n={Intervals}",
            request.FunctionExpression, request.LowerBound, request.UpperBound, request.Intervals);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var solver = env.Integration();

        var variant = (request.RectangleVariant ?? Shared.Enums.Integration.RectangleVariant.Midpoint).ToString();
        var jsonEnvelope = solver.SolveRectangle(
            request.FunctionExpression!,
            request.LowerBound,
            request.UpperBound,
            request.Intervals,
            variant
        );

        var result = jsonEnvelope.UnwrapOrThrow<IntegrationData>();
        stopwatch.Stop();

        logger.LogInformation("Rectangle completed: integral={Value}, elapsed={ElapsedMs}ms",
            result.IntegralValue, stopwatch.Elapsed.TotalMilliseconds);

        return MapToResponse(result, stopwatch.Elapsed.TotalMilliseconds);
    }

    public IntegrationResponse SolveTrapezoid(IntegrationRequest request)
    {
        logger.LogInformation("Trapezoid: f={Expression}, [{Lower}, {Upper}], n={Intervals}",
            request.FunctionExpression, request.LowerBound, request.UpperBound, request.Intervals);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var solver = env.Integration();

        var jsonEnvelope = solver.SolveTrapezoid(
            request.FunctionExpression!,
            request.LowerBound,
            request.UpperBound,
            request.Intervals
        );

        var result = jsonEnvelope.UnwrapOrThrow<IntegrationData>();
        stopwatch.Stop();

        logger.LogInformation("Trapezoid completed: integral={Value}, elapsed={ElapsedMs}ms",
            result.IntegralValue, stopwatch.Elapsed.TotalMilliseconds);

        return MapToResponse(result, stopwatch.Elapsed.TotalMilliseconds);
    }

    public IntegrationResponse SolveSimpson(IntegrationRequest request)
    {
        logger.LogInformation("Simpson: f={Expression}, [{Lower}, {Upper}], n={Intervals}",
            request.FunctionExpression, request.LowerBound, request.UpperBound, request.Intervals);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var solver = env.Integration();

        var jsonEnvelope = solver.SolveSimpson(
            request.FunctionExpression!,
            request.LowerBound,
            request.UpperBound,
            request.Intervals
        );

        var result = jsonEnvelope.UnwrapOrThrow<IntegrationData>();
        stopwatch.Stop();

        logger.LogInformation("Simpson completed: integral={Value}, elapsed={ElapsedMs}ms",
            result.IntegralValue, stopwatch.Elapsed.TotalMilliseconds);

        return MapToResponse(result, stopwatch.Elapsed.TotalMilliseconds);
    }

    public IntegrationComparisonResponse Compare(IntegrationComparisonRequest request)
    {
        logger.LogInformation("Compare: f={Expression}, [{Lower}, {Upper}], n={Intervals}, methods={Methods}",
            request.FunctionExpression, request.LowerBound, request.UpperBound, request.Intervals,
            string.Join(", ", request.Methods ?? []));

        var solver = env.Integration();
        var response = new IntegrationComparisonResponse();
        var stopwatch = new Stopwatch();

        foreach (var method in request.Methods ?? [])
        {
            var item = new IntegrationBenchmarkResultDto { Method = method };

            try
            {
                stopwatch.Restart();

                var jsonEnvelope = method switch
                {
                    IntegrationComparisonMethod.RectangleLeft =>
                        solver.SolveRectangle(request.FunctionExpression, request.LowerBound, request.UpperBound, request.Intervals, "Left"),
                    IntegrationComparisonMethod.RectangleRight =>
                        solver.SolveRectangle(request.FunctionExpression, request.LowerBound, request.UpperBound, request.Intervals, "Right"),
                    IntegrationComparisonMethod.RectangleMidpoint =>
                        solver.SolveRectangle(request.FunctionExpression, request.LowerBound, request.UpperBound, request.Intervals, "Midpoint"),
                    IntegrationComparisonMethod.Trapezoid =>
                        solver.SolveTrapezoid(request.FunctionExpression, request.LowerBound, request.UpperBound, request.Intervals),
                    IntegrationComparisonMethod.Simpson =>
                        solver.SolveSimpson(request.FunctionExpression, request.LowerBound, request.UpperBound, request.Intervals),
                    _ => throw new ArgumentOutOfRangeException(nameof(method))
                };

                var data = jsonEnvelope.UnwrapOrThrow<IntegrationData>();
                stopwatch.Stop();

                item.IntegralValue = data.IntegralValue;
                item.ExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds;

                logger.LogInformation("Compare/{Method}: integral={Value}, elapsed={ElapsedMs}ms",
                    method, data.IntegralValue, stopwatch.Elapsed.TotalMilliseconds);
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
            .Where(r => r.IntegralValue.HasValue)
            .OrderBy(r => r.ExecutionTimeMs)
            .FirstOrDefault()?.Method;

        logger.LogInformation("Compare completed: best method={BestMethod}", response.BestMethod);

        return response;
    }

    private static IntegrationResponse MapToResponse(IntegrationData data, double executionTimeMs) =>
        new()
        {
            IntegralValue = data.IntegralValue,
            ChartData = data.ChartPoints,
            ShapePoints = data.ShapePoints,
            SolutionSteps = data.SolutionSteps,
            ExecutionTimeMs = executionTimeMs
        };
}
