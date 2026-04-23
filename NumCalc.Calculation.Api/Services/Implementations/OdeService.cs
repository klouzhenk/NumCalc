using System.Diagnostics;
using CSnakes.Runtime;
using Microsoft.Extensions.Logging;
using NumCalc.Calculation.Api.Entities.ODE;
using NumCalc.Calculation.Api.Services.Interfaces;
using NumCalc.Calculation.Api.Utils;
using NumCalc.Shared.DTOs.ODE;
using NumCalc.Shared.Enums.ODE;
using NumCalc.Shared.ODE.Requests;
using NumCalc.Shared.ODE.Responses;

namespace NumCalc.Calculation.Api.Services.Implementations;

public class OdeService(IPythonEnvironment env, ILogger<OdeService> logger) : IOdeService
{
    public OdeResponse SolveEuler(OdeRequest request)
    {
        logger.LogInformation("Euler: f={Expression}, x0={X0}, y0={Y0}, target={TargetX}, h={StepSize}",
            request.FunctionExpression, request.InitialX, request.InitialY, request.TargetX, request.StepSize);

        var stopWatch = System.Diagnostics.Stopwatch.StartNew();
        var solver = env.Ode();

        var jsonEnvelope = solver.SolveEuler(
            request.FunctionExpression ?? string.Empty,
            request.InitialX,
            request.InitialY,
            request.TargetX,
            request.StepSize
        );

        var result = jsonEnvelope.UnwrapOrThrow<OdeData>();
        stopWatch.Stop();

        logger.LogInformation("Euler completed: {Points} points, elapsed={ElapsedMs}ms",
            result.SolutionPoints?.Count, stopWatch.Elapsed.TotalMilliseconds);

        return MapToResponse(result, stopWatch.Elapsed.TotalMilliseconds);
    }

    public OdeResponse SolveEulerImproved(OdeRequest request)
    {
        logger.LogInformation("EulerImproved: f={Expression}, x0={X0}, y0={Y0}, target={TargetX}, h={StepSize}",
            request.FunctionExpression, request.InitialX, request.InitialY, request.TargetX, request.StepSize);

        var stopWatch = System.Diagnostics.Stopwatch.StartNew();
        var solver = env.Ode();

        var jsonEnvelope = solver.SolveEulerImproved(
            request.FunctionExpression ?? string.Empty,
            request.InitialX,
            request.InitialY,
            request.TargetX,
            request.StepSize
        );

        var result = jsonEnvelope.UnwrapOrThrow<OdeData>();
        stopWatch.Stop();

        logger.LogInformation("EulerImproved completed: {Points} points, elapsed={ElapsedMs}ms",
            result.SolutionPoints?.Count, stopWatch.Elapsed.TotalMilliseconds);

        return MapToResponse(result, stopWatch.Elapsed.TotalMilliseconds);
    }

    public OdeResponse SolveRungeKutta2(OdeRequest request)
    {
        logger.LogInformation("RK2: f={Expression}, x0={X0}, y0={Y0}, target={TargetX}, h={StepSize}",
            request.FunctionExpression, request.InitialX, request.InitialY, request.TargetX, request.StepSize);

        var stopWatch = System.Diagnostics.Stopwatch.StartNew();
        var solver = env.Ode();

        var jsonEnvelope = solver.SolveRungeKutta2(
            request.FunctionExpression ?? string.Empty,
            request.InitialX,
            request.InitialY,
            request.TargetX,
            request.StepSize
        );

        var result = jsonEnvelope.UnwrapOrThrow<OdeData>();
        stopWatch.Stop();

        logger.LogInformation("RK2 completed: {Points} points, elapsed={ElapsedMs}ms",
            result.SolutionPoints?.Count, stopWatch.Elapsed.TotalMilliseconds);

        return MapToResponse(result, stopWatch.Elapsed.TotalMilliseconds);
    }

    public OdeResponse SolveRungeKutta4(OdeRequest request)
    {
        logger.LogInformation("RK4: f={Expression}, x0={X0}, y0={Y0}, target={TargetX}, h={StepSize}",
            request.FunctionExpression, request.InitialX, request.InitialY, request.TargetX, request.StepSize);

        var stopWatch = System.Diagnostics.Stopwatch.StartNew();
        var solver = env.Ode();

        var jsonEnvelope = solver.SolveRungeKutta4(
            request.FunctionExpression ?? string.Empty,
            request.InitialX,
            request.InitialY,
            request.TargetX,
            request.StepSize
        );

        var result = jsonEnvelope.UnwrapOrThrow<OdeData>();
        stopWatch.Stop();

        logger.LogInformation("RK4 completed: {Points} points, elapsed={ElapsedMs}ms",
            result.SolutionPoints?.Count, stopWatch.Elapsed.TotalMilliseconds);

        return MapToResponse(result, stopWatch.Elapsed.TotalMilliseconds);
    }

    public OdeResponse SolvePicard(OdeRequest request)
    {
        logger.LogInformation("Picard: f={Expression}, x0={X0}, y0={Y0}, target={TargetX}, h={StepSize}, order={Order}",
            request.FunctionExpression, request.InitialX, request.InitialY, request.TargetX, request.StepSize, request.PicardOrder);

        var stopWatch = System.Diagnostics.Stopwatch.StartNew();
        var solver = env.Ode();

        var jsonEnvelope = solver.SolvePicard(
            request.FunctionExpression ?? string.Empty,
            request.InitialX,
            request.InitialY,
            request.TargetX,
            request.StepSize,
            request.PicardOrder
        );

        var result = jsonEnvelope.UnwrapOrThrow<OdeData>();
        stopWatch.Stop();

        logger.LogInformation("Picard completed: {Points} points, elapsed={ElapsedMs}ms",
            result.SolutionPoints?.Count, stopWatch.Elapsed.TotalMilliseconds);

        return MapToResponse(result, stopWatch.Elapsed.TotalMilliseconds);
    }

    public OdeComparisonResponse Compare(OdeComparisonRequest request)
    {
        logger.LogInformation("Compare: f={Expression}, x0={X0}, y0={Y0}, target={TargetX}, h={StepSize}, methods={Methods}",
            request.FunctionExpression, request.InitialX, request.InitialY, request.TargetX, request.StepSize,
            string.Join(", ", request.Methods ?? []));

        var solver = env.Ode();
        var response = new OdeComparisonResponse();
        var stopwatch = new Stopwatch();

        foreach (var method in request.Methods ?? [])
        {
            var item = new OdeBenchmarkResultDto { Method = method };

            try
            {
                stopwatch.Restart();

                var jsonEnvelope = method switch
                {
                    OdeMethod.Euler =>
                        solver.SolveEuler(request.FunctionExpression ?? string.Empty, request.InitialX, request.InitialY, request.TargetX, request.StepSize),
                    OdeMethod.EulerImproved =>
                        solver.SolveEulerImproved(request.FunctionExpression ?? string.Empty, request.InitialX, request.InitialY, request.TargetX, request.StepSize),
                    OdeMethod.RungeKutta2 =>
                        solver.SolveRungeKutta2(request.FunctionExpression ?? string.Empty, request.InitialX, request.InitialY, request.TargetX, request.StepSize),
                    OdeMethod.RungeKutta4 =>
                        solver.SolveRungeKutta4(request.FunctionExpression ?? string.Empty, request.InitialX, request.InitialY, request.TargetX, request.StepSize),
                    _ => throw new ArgumentOutOfRangeException(nameof(method))
                };

                var data = jsonEnvelope.UnwrapOrThrow<OdeData>();
                stopwatch.Stop();

                item.FinalY = data.SolutionPoints?.LastOrDefault()?.Y;
                item.ExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds;

                logger.LogInformation("Compare/{Method}: finalY={FinalY}, elapsed={ElapsedMs}ms",
                    method, item.FinalY, item.ExecutionTimeMs);
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
            .Where(r => r.FinalY.HasValue)
            .OrderBy(r => r.ExecutionTimeMs)
            .FirstOrDefault()?.Method;

        logger.LogInformation("Compare completed: best method={BestMethod}", response.BestMethod);

        return response;
    }

    private static OdeResponse MapToResponse(OdeData data, double executionTimeMs) =>
        new()
        {
            SolutionPoints = data.SolutionPoints,
            PolynomialLatex = data.PolynomialLatex,
            SolutionSteps = data.SolutionSteps,
            ExecutionTimeMs = executionTimeMs
        };
}