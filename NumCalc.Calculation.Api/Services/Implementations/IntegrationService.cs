using CSnakes.Runtime;
using NumCalc.Calculation.Api.Entities.Integration;
using NumCalc.Calculation.Api.Services.Interfaces;
using NumCalc.Calculation.Api.Utils;
using NumCalc.Shared.Integration.Requests;
using NumCalc.Shared.Integration.Responses;

namespace NumCalc.Calculation.Api.Services.Implementations;

public class IntegrationService(IPythonEnvironment env) : IIntegrationService
{
    public IntegrationResponse SolveRectangle(IntegrationRequest request)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var solver = env.Integration();

        var jsonEnvelope = solver.SolveRectangle(
            request.FunctionExpression!,
            request.LowerBound,
            request.UpperBound,
            request.Intervals
        );

        var result = jsonEnvelope.UnwrapOrThrow<IntegrationData>();
        stopwatch.Stop();

        return MapToResponse(result, stopwatch.Elapsed.TotalMilliseconds);
    }

    public IntegrationResponse SolveTrapezoid(IntegrationRequest request)
    {
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

        return MapToResponse(result, stopwatch.Elapsed.TotalMilliseconds);
    }

    public IntegrationResponse SolveSimpson(IntegrationRequest request)
    {
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

        return MapToResponse(result, stopwatch.Elapsed.TotalMilliseconds);
    }

    private static IntegrationResponse MapToResponse(IntegrationData data, double executionTimeMs) =>
        new()
        {
            IntegralValue = data.IntegralValue,
            ChartData = data.ChartPoints,
            SolutionSteps = data.SolutionSteps,
            ExecutionTimeMs = executionTimeMs
        };
}
