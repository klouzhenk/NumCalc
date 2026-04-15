using CSnakes.Runtime;
using NumCalc.Calculation.Api.Entities.ODE;
using NumCalc.Calculation.Api.Services.Interfaces;
using NumCalc.Calculation.Api.Utils;
using NumCalc.Shared.ODE.Requests;
using NumCalc.Shared.ODE.Responses;

namespace NumCalc.Calculation.Api.Services.Implementations;

public class OdeService(IPythonEnvironment env) : IOdeService
{
    public OdeResponse SolveEuler(OdeRequest request)
    {
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

        return MapToResponse(result, stopWatch.ElapsedMilliseconds);
    }

    public OdeResponse SolveEulerImproved(OdeRequest request)
    {
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

        return MapToResponse(result, stopWatch.ElapsedMilliseconds);
    }

    public OdeResponse SolveRungeKutta2(OdeRequest request)
    {
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

        return MapToResponse(result, stopWatch.ElapsedMilliseconds);
    }

    public OdeResponse SolveRungeKutta4(OdeRequest request)
    {
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

        return MapToResponse(result, stopWatch.ElapsedMilliseconds);
    }

    public OdeResponse SolvePicard(OdeRequest request)
    {
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

        return MapToResponse(result, stopWatch.ElapsedMilliseconds);
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