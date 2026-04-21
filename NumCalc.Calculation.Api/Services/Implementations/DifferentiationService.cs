using CSnakes.Runtime;
using Microsoft.Extensions.Logging;
using NumCalc.Calculation.Api.Entities.Differentiation;
using NumCalc.Calculation.Api.Services.Interfaces;
using NumCalc.Calculation.Api.Utils;
using NumCalc.Shared.Differentiation.Requests;
using NumCalc.Shared.Differentiation.Responses;
using NumCalc.Shared.Enums.Differentiation;

namespace NumCalc.Calculation.Api.Services.Implementations;

public class DifferentiationService(IPythonEnvironment env, ILogger<DifferentiationService> logger) : IDifferentiationService
{
    public DifferentiationResponse SolveFiniteDiff(DifferentiationRequest request)
    {
        logger.LogInformation("FiniteDiff: f={Expression}, x={QueryPoint}, h={StepSize}, order={Order}",
            request.FunctionExpression, request.QueryPoint, request.StepSize, request.DerivativeOrder);

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

        logger.LogInformation("FiniteDiff completed: derivative={Value}, elapsed={ElapsedMs}ms",
            result.DerivativeValue, stopwatch.Elapsed.TotalMilliseconds);

        return MapToResponse(result, stopwatch.Elapsed.TotalMilliseconds);
    }

    public DifferentiationResponse SolveLagrange(DifferentiationRequest request)
    {
        logger.LogInformation("Lagrange differentiation: mode={Mode}, nodes={NodeCount}, x={QueryPoint}",
            request.Mode, request.XNodes?.Count, request.QueryPoint);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var solver = env.Differentiation();

        var jsonEnvelope = solver.SolveLagrange(
            request.XNodes!,
            request.Mode == DifferentiationInputMode.RawData ? request.YValues : null,
            request.QueryPoint,
            request.Mode == DifferentiationInputMode.Function ? request.FunctionExpression : null,
            request.DerivativeOrder
        );

        var result = jsonEnvelope.UnwrapOrThrow<DifferentiationData>();
        stopwatch.Stop();

        logger.LogInformation("Lagrange differentiation completed: derivative={Value}, elapsed={ElapsedMs}ms",
            result.DerivativeValue, stopwatch.Elapsed.TotalMilliseconds);

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