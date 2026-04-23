using System.Diagnostics;
using CSnakes.Runtime;
using NumCalc.Calculation.Api.Entities.Differentiation;
using NumCalc.Calculation.Api.Services.Interfaces;
using NumCalc.Calculation.Api.Utils;
using NumCalc.Shared.Differentiation.Requests;
using NumCalc.Shared.Differentiation.Responses;
using NumCalc.Shared.DTOs.Differentiation;
using NumCalc.Shared.Enums.Differentiation;

namespace NumCalc.Calculation.Api.Services.Implementations;

public class DifferentiationService(IPythonEnvironment env, ILogger<DifferentiationService> logger) : IDifferentiationService
{
    public DifferentiationResponse SolveForward(DifferentiationRequest request)
    {
        logger.LogInformation("Forward: f={Expression}, x={QueryPoint}, h={StepSize}, order={Order}",
            request.FunctionExpression, request.QueryPoint, request.StepSize, request.DerivativeOrder);

        var stopwatch = Stopwatch.StartNew();
        var jsonEnvelope = env.Differentiation().SolveFiniteDiffForward(
            request.FunctionExpression!, request.QueryPoint, request.StepSize, request.DerivativeOrder);
        var result = jsonEnvelope.UnwrapOrThrow<DifferentiationData>();
        stopwatch.Stop();

        logger.LogInformation("Forward completed: derivative={Value}, elapsed={ElapsedMs}ms",
            result.DerivativeValue, stopwatch.Elapsed.TotalMilliseconds);

        return MapToResponse(result, stopwatch.Elapsed.TotalMilliseconds);
    }

    public DifferentiationResponse SolveBackward(DifferentiationRequest request)
    {
        logger.LogInformation("Backward: f={Expression}, x={QueryPoint}, h={StepSize}, order={Order}",
            request.FunctionExpression, request.QueryPoint, request.StepSize, request.DerivativeOrder);

        var stopwatch = Stopwatch.StartNew();
        var jsonEnvelope = env.Differentiation().SolveFiniteDiffBackward(
            request.FunctionExpression!, request.QueryPoint, request.StepSize, request.DerivativeOrder);
        var result = jsonEnvelope.UnwrapOrThrow<DifferentiationData>();
        stopwatch.Stop();

        logger.LogInformation("Backward completed: derivative={Value}, elapsed={ElapsedMs}ms",
            result.DerivativeValue, stopwatch.Elapsed.TotalMilliseconds);

        return MapToResponse(result, stopwatch.Elapsed.TotalMilliseconds);
    }

    public DifferentiationResponse SolveCentral(DifferentiationRequest request)
    {
        logger.LogInformation("Central: f={Expression}, x={QueryPoint}, h={StepSize}, order={Order}",
            request.FunctionExpression, request.QueryPoint, request.StepSize, request.DerivativeOrder);

        var stopwatch = Stopwatch.StartNew();
        var jsonEnvelope = env.Differentiation().SolveFiniteDiffCentral(
            request.FunctionExpression!, request.QueryPoint, request.StepSize, request.DerivativeOrder);
        var result = jsonEnvelope.UnwrapOrThrow<DifferentiationData>();
        stopwatch.Stop();

        logger.LogInformation("Central completed: derivative={Value}, elapsed={ElapsedMs}ms",
            result.DerivativeValue, stopwatch.Elapsed.TotalMilliseconds);

        return MapToResponse(result, stopwatch.Elapsed.TotalMilliseconds);
    }

    public DifferentiationResponse SolveLagrange(DifferentiationRequest request)
    {
        logger.LogInformation("Lagrange: mode={Mode}, nodes={NodeCount}, x={QueryPoint}, order={Order}",
            request.Mode, request.XNodes?.Count, request.QueryPoint, request.DerivativeOrder);

        var stopwatch = Stopwatch.StartNew();
        var jsonEnvelope = env.Differentiation().SolveLagrange(
            request.XNodes!,
            request.Mode == DifferentiationInputMode.RawData ? request.YValues : null,
            request.QueryPoint,
            request.Mode == DifferentiationInputMode.Function ? request.FunctionExpression : null,
            request.DerivativeOrder);
        var result = jsonEnvelope.UnwrapOrThrow<DifferentiationData>();
        stopwatch.Stop();

        logger.LogInformation("Lagrange completed: derivative={Value}, elapsed={ElapsedMs}ms",
            result.DerivativeValue, stopwatch.Elapsed.TotalMilliseconds);

        return MapToResponse(result, stopwatch.Elapsed.TotalMilliseconds);
    }

    public DifferentiationComparisonResponse Compare(DifferentiationComparisonRequest request)
    {
        logger.LogInformation("Compare: f={Expression}, x={QueryPoint}, h={StepSize}, order={Order}, methods={Methods}",
            request.FunctionExpression, request.QueryPoint, request.StepSize, request.DerivativeOrder,
            string.Join(", ", request.Methods ?? []));

        var solver = env.Differentiation();
        var response = new DifferentiationComparisonResponse();
        var stopwatch = new Stopwatch();

        foreach (var method in request.Methods ?? [])
        {
            var item = new DifferentiationBenchmarkResultDto { Method = method };

            try
            {
                stopwatch.Restart();

                var jsonEnvelope = method switch
                {
                    DifferentiationComparisonMethod.Forward =>
                        solver.SolveFiniteDiffForward(request.FunctionExpression, request.QueryPoint, request.StepSize, request.DerivativeOrder),
                    DifferentiationComparisonMethod.Backward =>
                        solver.SolveFiniteDiffBackward(request.FunctionExpression, request.QueryPoint, request.StepSize, request.DerivativeOrder),
                    DifferentiationComparisonMethod.Central =>
                        solver.SolveFiniteDiffCentral(request.FunctionExpression, request.QueryPoint, request.StepSize, request.DerivativeOrder),
                    DifferentiationComparisonMethod.Lagrange =>
                        solver.SolveLagrange(request.XNodes!, null, request.QueryPoint, request.FunctionExpression, request.DerivativeOrder),
                    _ => throw new ArgumentOutOfRangeException(nameof(method))
                };

                var data = jsonEnvelope.UnwrapOrThrow<DifferentiationData>();
                stopwatch.Stop();

                item.DerivativeValue = data.DerivativeValue;
                item.ExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds;

                logger.LogInformation("Compare/{Method}: derivative={Value}, elapsed={ElapsedMs}ms",
                    method, item.DerivativeValue, item.ExecutionTimeMs);
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
            .Where(r => r.DerivativeValue.HasValue)
            .OrderBy(r => r.ExecutionTimeMs)
            .FirstOrDefault()?.Method;

        logger.LogInformation("Compare completed: best method={BestMethod}", response.BestMethod);

        return response;
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
