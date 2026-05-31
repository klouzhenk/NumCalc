using System.Diagnostics;
using CSnakes.Runtime;
using Microsoft.Extensions.Logging;
using NumCalc.Calculation.Business.Entities.EquationSystems;
using NumCalc.Calculation.Business.Exceptions;
using NumCalc.Calculation.Business.Services.Interfaces;
using NumCalc.Calculation.Business.Utils;
using NumCalc.Shared.DTOs.EquationSystems;
using NumCalc.Shared.Enums;
using NumCalc.Shared.Enums.EquationSystems;
using NumCalc.Shared.EquationsSystems.Requests;
using NumCalc.Shared.EquationsSystems.Responses;

namespace NumCalc.Calculation.Business.Services.Implementations;

public class EquationsSystemService(IPythonEnvironment env, ILogger<EquationsSystemService> logger) : IEquationsSystemService
{
    public SystemSolvingResponse SolveCramer(SystemSolvingRequest request)
    {
        if (request.Equations == null || request.Equations.Count == 0)
            throw new CustomException(NumCalcErrorCode.SyntaxError, "The entered equations list is empty");

        logger.LogInformation("Cramer: {Count} equations, variables={Variables}",
            request.Equations.Count, string.Join(", ", request.Variables));

        var equationSystemSolver = env.EquationSystems();
        var jsonEnvelope = equationSystemSolver.SolveCramer(request.Equations, request.Variables);
        var result = jsonEnvelope.UnwrapOrThrow<SystemSolvingData>();

        logger.LogInformation("Cramer completed: {Count} roots", result.Roots?.Count);

        return new SystemSolvingResponse
        {
            Roots = result.Roots,
            ChartSeries = result.ChartSeries,
            SolutionSteps = result.SolutionSteps
        };
    }

    public SystemSolvingResponse SolveGaussian(SystemSolvingRequest request)
    {
        if (request.Equations == null || request.Equations.Count == 0)
            throw new CustomException(NumCalcErrorCode.SyntaxError, "The entered equations list is empty");

        logger.LogInformation("Gaussian: {Count} equations, variables={Variables}",
            request.Equations.Count, string.Join(", ", request.Variables));

        var equationSystemSolver = env.EquationSystems();
        var jsonEnvelope = equationSystemSolver.SolveGaussian(request.Equations, request.Variables);
        var result = jsonEnvelope.UnwrapOrThrow<SystemSolvingData>();

        logger.LogInformation("Gaussian completed: {Count} roots", result.Roots?.Count);

        return new SystemSolvingResponse
        {
            Roots = result.Roots,
            ChartSeries = result.ChartSeries,
            SolutionSteps = result.SolutionSteps
        };
    }

    public SystemSolvingResponse SolveFixedPoint(NonLinearSystemRequest request)
    {
        if (request.IterationFunctions == null || request.IterationFunctions.Count == 0)
            throw new CustomException(NumCalcErrorCode.SyntaxError, "The iteration functions list is empty");

        logger.LogInformation("FixedPoint: {Count} functions, tol={Tolerance}, maxIter={MaxIterations}",
            request.IterationFunctions.Count, request.Tolerance, request.MaxIterations);

        var equationSystemSolver = env.EquationSystems();
        var jsonEnvelope = equationSystemSolver.SolveFixedPoint(
            request.IterationFunctions,
            request.Variables,
            request.InitialGuess,
            request.Tolerance,
            request.MaxIterations
        );
        var result = jsonEnvelope.UnwrapOrThrow<SystemSolvingData>();

        logger.LogInformation("FixedPoint completed: {Count} roots", result.Roots?.Count);

        return new SystemSolvingResponse
        {
            Roots = result.Roots,
            ChartSeries = result.ChartSeries,
            SolutionSteps = result.SolutionSteps
        };
    }

    public SystemSolvingResponse SolveSeidel(NonLinearSystemRequest request)
    {
        if (request.IterationFunctions == null || request.IterationFunctions.Count == 0)
            throw new CustomException(NumCalcErrorCode.SyntaxError, "The iteration functions list is empty");

        logger.LogInformation("Seidel: {Count} functions, tol={Tolerance}, maxIter={MaxIterations}",
            request.IterationFunctions.Count, request.Tolerance, request.MaxIterations);

        var equationSystemSolver = env.EquationSystems();
        var jsonEnvelope = equationSystemSolver.SolveSeidel(
            request.IterationFunctions,
            request.Variables,
            request.InitialGuess,
            request.Tolerance,
            request.MaxIterations
        );
        var result = jsonEnvelope.UnwrapOrThrow<SystemSolvingData>();

        logger.LogInformation("Seidel completed: {Count} roots", result.Roots?.Count);

        return new SystemSolvingResponse
        {
            Roots = result.Roots,
            ChartSeries = result.ChartSeries,
            SolutionSteps = result.SolutionSteps
        };
    }

    public LinearSystemComparisonResponse CompareLinear(LinearSystemComparisonRequest request)
    {
        logger.LogInformation("LinearComparison: {Count} equations", request.Equations.Count);

        var solver = env.EquationSystems();
        var methods = request.Methods?.ToList() is { Count: > 0 } m
            ? m
            : Enum.GetValues<LinearSystemMethod>().ToList();

        var results = new List<LinearSystemBenchmarkResultDto>();
        var sw = new Stopwatch();

        foreach (var method in methods)
        {
            var item = new LinearSystemBenchmarkResultDto { Method = method };

            try
            {
                sw.Restart();

                var jsonEnvelope = method switch
                {
                    LinearSystemMethod.Cramer => solver.SolveCramer(request.Equations, request.Variables),
                    LinearSystemMethod.Gauss  => solver.SolveGaussian(request.Equations, request.Variables),
                    _ => throw new ArgumentOutOfRangeException(nameof(method))
                };
                sw.Stop();

                var data = jsonEnvelope.UnwrapOrThrow<SystemSolvingData>();

                item.Roots = data.Roots;
                item.ExecutionTimeMs = sw.Elapsed.TotalMilliseconds;

                logger.LogInformation("LinearComparison/{Method}: {Count} roots, elapsed={ElapsedMs}ms",
                    method, data.Roots?.Count, sw.Elapsed.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                sw.Stop();
                item.ExecutionTimeMs = sw.Elapsed.TotalMilliseconds;
                logger.LogWarning(ex, "LinearComparison/{Method} failed after {ElapsedMs}ms", method, sw.Elapsed.TotalMilliseconds);
            }

            results.Add(item);
        }

        var best = results
            .Where(r => r.Roots is not null)
            .OrderBy(r => r.ExecutionTimeMs)
            .FirstOrDefault();

        return new LinearSystemComparisonResponse
        {
            Results = results,
            BestMethod = best?.Method
        };
    }

    public NonLinearSystemComparisonResponse CompareNonLinear(NonLinearSystemComparisonRequest request)
    {
        logger.LogInformation("NonLinearComparison: {Count} functions", request.IterationFunctions.Count);

        var solver = env.EquationSystems();
        var methods = request.Methods?.ToList() is { Count: > 0 } m
            ? m
            : Enum.GetValues<NonLinearSystemMethod>().ToList();

        var results = new List<NonLinearSystemBenchmarkResultDto>();
        var sw = new Stopwatch();

        foreach (var method in methods)
        {
            var item = new NonLinearSystemBenchmarkResultDto { Method = method };

            try
            {
                sw.Restart();

                var jsonEnvelope = method switch
                {
                    NonLinearSystemMethod.FixedPoint =>
                        solver.SolveFixedPoint(request.IterationFunctions, request.Variables, request.InitialGuess, request.Tolerance, request.MaxIterations),
                    NonLinearSystemMethod.Seidel =>
                        solver.SolveSeidel(request.IterationFunctions, request.Variables, request.InitialGuess, request.Tolerance, request.MaxIterations),
                    _ => throw new ArgumentOutOfRangeException(nameof(method))
                };
                sw.Stop();

                var data = jsonEnvelope.UnwrapOrThrow<SystemSolvingData>();

                item.Roots = data.Roots;
                item.ExecutionTimeMs = sw.Elapsed.TotalMilliseconds;

                logger.LogInformation("NonLinearComparison/{Method}: {Count} roots, elapsed={ElapsedMs}ms",
                    method, data.Roots?.Count, sw.Elapsed.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                sw.Stop();
                item.ExecutionTimeMs = sw.Elapsed.TotalMilliseconds;
                logger.LogWarning(ex, "NonLinearComparison/{Method} failed after {ElapsedMs}ms", method, sw.Elapsed.TotalMilliseconds);
            }

            results.Add(item);
        }

        var best = results
            .Where(r => r.Roots is not null)
            .OrderBy(r => r.ExecutionTimeMs)
            .FirstOrDefault();

        return new NonLinearSystemComparisonResponse
        {
            Results = results,
            BestMethod = best?.Method
        };
    }
}