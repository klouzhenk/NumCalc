using System.Diagnostics;
using CSnakes.Runtime;
using NumCalc.Calculation.Api.Entities.Interpolation;
using NumCalc.Calculation.Api.Services.Interfaces;
using NumCalc.Calculation.Api.Utils;
using NumCalc.Shared.DTOs.Interpolation;
using NumCalc.Shared.Enums.Interpolation;
using NumCalc.Shared.Interpolation.Requests;
using NumCalc.Shared.Interpolation.Responses;

namespace NumCalc.Calculation.Api.Services.Implementations;

public class InterpolationService(IPythonEnvironment env, ILogger<InterpolationService> logger) : IInterpolationService
{
    public InterpolationResponse SolveNewton(InterpolationRequest request)
    {
        logger.LogInformation("Newton interpolation: mode={Mode}, nodes={NodeCount}, queryPoint={QueryPoint}",
            request.Mode, request.XNodes.Count, request.QueryPoint);

        var stopwatch = Stopwatch.StartNew();
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
            request.Mode, request.XNodes.Count, request.QueryPoint);

        var stopwatch = Stopwatch.StartNew();
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
            request.Mode, request.XNodes.Count, request.QueryPoint);

        var stopwatch = Stopwatch.StartNew();
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

    public InterpolationComparisonResponse Compare(InterpolationComparisonRequest request)
    {
        if (request.Mode is InterpolationInputMode.Function)
        {
            logger.LogInformation("Compare: f={Expression} at the points [{XNodes}] within query point = {QueryPoint}. The methods - {Methods}",
                request.FunctionExpression, string.Join(", ", request.XNodes), request.QueryPoint,
                string.Join(", ", request.Methods ?? []));
        }
        else
        {
            logger.LogInformation("Compare: function nodes [{nodes}] within query point = {QueryPoint}. The methods - {Methods}",
                string.Join(", ", request.XNodes), request.QueryPoint, string.Join(", ", request.Methods ?? []));
        }
        
        var solver = env.Interpolation();
        var response = new InterpolationComparisonResponse();
        var stopwatch = new Stopwatch();

        foreach (var method in request.Methods ?? [])
        {
            var item = new InterpolationBenchmarkResultDto() { Method = method };

            try
            {
                stopwatch.Restart();

                var jsonEnvelope = method switch
                {
                    InterpolationMethod.Lagrange => 
                        solver.SolveLagrange(request.XNodes, request.YValues, request.QueryPoint, request.FunctionExpression),
                    InterpolationMethod.Newton => 
                        solver.SolveNewton(request.XNodes, request.YValues, request.QueryPoint, request.FunctionExpression),
                    InterpolationMethod.Spline 
                        => solver.SolveSpline(request.XNodes, request.YValues, request.QueryPoint, request.FunctionExpression),
                    _ => throw new ArgumentOutOfRangeException(nameof(method))
                };

                var data = jsonEnvelope.UnwrapOrThrow<InterpolationData>();
                stopwatch.Stop();

                item.InterpolatedValue = data.InterpolatedValue;
                item.ExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds;
                
                logger.LogInformation("Compare/{Method}: interpolated value={Value}, elapsed={ElapsedMs}ms",
                    method, data.InterpolatedValue, stopwatch.Elapsed.TotalMilliseconds);
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
            .Where(r => r.InterpolatedValue.HasValue)
            .OrderBy(r => r.ExecutionTimeMs)
            .FirstOrDefault()?.Method;

        logger.LogInformation("Compare completed: best method={BestMethod}", response.BestMethod);

        return response;
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
