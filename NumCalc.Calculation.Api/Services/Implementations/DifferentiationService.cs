using CSnakes.Runtime;
using NumCalc.Calculation.Api.Entities.Differentiation;
using NumCalc.Calculation.Api.Services.Interfaces;
using NumCalc.Calculation.Api.Utils;
using NumCalc.Shared.Differentiation.Requests;
using NumCalc.Shared.Differentiation.Responses;
using NumCalc.Shared.Enums.Differentiation;

namespace NumCalc.Calculation.Api.Services.Implementations;

public class DifferentiationService(IPythonEnvironment env) : IDifferentiationService
{
    public DifferentiationResponse SolveFiniteDiff(DifferentiationRequest request)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var solver = env.Differentiation();

        var jsonEnvelope = solver.SolveFiniteDiff(
            request.FunctionExpression!,
            request.QueryPoint,
            request.StepSize,
            request.DerivativeOrder
        );

        var result = jsonEnvelope.UnwrapOrThrow<DifferentiationData>();
        stopwatch.Stop();

        return MapToResponse(result, stopwatch.Elapsed.TotalMilliseconds);
    }

    public DifferentiationResponse SolveLagrange(DifferentiationRequest request)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var solver = env.Differentiation();

        var jsonEnvelope = solver.SolveLagrange(
            request.XNodes!,
            request.Mode == DifferentiationInputMode.RawData ? request.YValues : null,
            request.QueryPoint,
            request.Mode == DifferentiationInputMode.Function ? request.FunctionExpression : null
        );

        var result = jsonEnvelope.UnwrapOrThrow<DifferentiationData>();
        stopwatch.Stop();

        return MapToResponse(result, stopwatch.Elapsed.TotalMilliseconds);
    }

    private static DifferentiationResponse MapToResponse(DifferentiationData data, double executionTimeMs) =>
        new()
        {
            DerivativeValue = data.DerivativeValue,
            PolynomialLatex = data.PolynomialLatex,
            ChartData = data.ChartPoints,
            SolutionSteps = data.SolutionSteps,
            ExecutionTimeMs = executionTimeMs
        };
}