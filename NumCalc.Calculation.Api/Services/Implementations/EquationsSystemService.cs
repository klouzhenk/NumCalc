using System.Diagnostics;
using CSnakes.Runtime;
using Microsoft.Extensions.Logging;
using NumCalc.Calculation.Api.Entities;
using NumCalc.Calculation.Api.Entities.EquationSystems;
using NumCalc.Calculation.Api.Exceptions;
using NumCalc.Calculation.Api.Services.Interfaces;
using NumCalc.Calculation.Api.Utils;
using NumCalc.Shared.DTOs.EquationSystems;
using NumCalc.Shared.Enums;
using NumCalc.Shared.Enums.EquationSystems;
using NumCalc.Shared.EquationsSystems.Requests;
using NumCalc.Shared.EquationsSystems.Responses;

namespace NumCalc.Calculation.Api.Services.Implementations;

public class EquationsSystemService(IPythonEnvironment env, ILogger<EquationsSystemService> logger) : IEquationsSystemService
{
    public SystemSolvingResponse SolveCramer(SystemSolvingRequest request)
    {
        if (request.Equations == null || request.Equations.Count == 0)
            throw new CustomException(NumCalcErrorCode.SyntaxError, "The entered equations list is empty");

        logger.LogInformation("Cramer: {Count} equations, variables={Variables}",
            request.Equations.Count, string.Join(", ", request.Variables ?? []));

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
            request.Equations.Count, string.Join(", ", request.Variables ?? []));

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

        var solverRequest = new SystemSolvingRequest
        {
            Equations = request.Equations,
            Variables = request.Variables
        };

        var results = new List<LinearSystemBenchmarkResultDto>();

        var sw = Stopwatch.StartNew();
        var cramerResult = SolveCramer(solverRequest);
        sw.Stop();
        results.Add(new LinearSystemBenchmarkResultDto
        {
            Method = LinearSystemComparisonMethod.Cramer,
            Roots = cramerResult.Roots,
            ExecutionTimeMs = sw.Elapsed.TotalMilliseconds
        });

        sw.Restart();
        var gaussResult = SolveGaussian(solverRequest);
        sw.Stop();
        results.Add(new LinearSystemBenchmarkResultDto
        {
            Method = LinearSystemComparisonMethod.Gauss,
            Roots = gaussResult.Roots,
            ExecutionTimeMs = sw.Elapsed.TotalMilliseconds
        });

        var best = results.OrderBy(r => r.ExecutionTimeMs).FirstOrDefault();

        return new LinearSystemComparisonResponse
        {
            Results = results,
            BestMethod = best?.Method
        };
    }

    public NonLinearSystemComparisonResponse CompareNonLinear(NonLinearSystemComparisonRequest request)
    {
        logger.LogInformation("NonLinearComparison: {Count} functions", request.IterationFunctions.Count);

        var solverRequest = new NonLinearSystemRequest
        {
            IterationFunctions = request.IterationFunctions,
            Variables = request.Variables,
            InitialGuess = request.InitialGuess,
            Tolerance = request.Tolerance,
            MaxIterations = request.MaxIterations
        };

        var results = new List<NonLinearSystemBenchmarkResultDto>();

        var sw = Stopwatch.StartNew();
        var fixedPointResult = SolveFixedPoint(solverRequest);
        sw.Stop();
        results.Add(new NonLinearSystemBenchmarkResultDto
        {
            Method = NonLinearSystemComparisonMethod.FixedPoint,
            Roots = fixedPointResult.Roots,
            ExecutionTimeMs = sw.Elapsed.TotalMilliseconds
        });

        sw.Restart();
        var seidelResult = SolveSeidel(solverRequest);
        sw.Stop();
        results.Add(new NonLinearSystemBenchmarkResultDto
        {
            Method = NonLinearSystemComparisonMethod.Seidel,
            Roots = seidelResult.Roots,
            ExecutionTimeMs = sw.Elapsed.TotalMilliseconds
        });

        var best = results.OrderBy(r => r.ExecutionTimeMs).FirstOrDefault();

        return new NonLinearSystemComparisonResponse
        {
            Results = results,
            BestMethod = best?.Method
        };
    }
}