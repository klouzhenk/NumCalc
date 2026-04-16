using CSnakes.Runtime;
using Microsoft.Extensions.Logging;
using NumCalc.Calculation.Api.Entities;
using NumCalc.Calculation.Api.Entities.EquationSystems;
using NumCalc.Calculation.Api.Exceptions;
using NumCalc.Calculation.Api.Services.Interfaces;
using NumCalc.Calculation.Api.Utils;
using NumCalc.Shared.Enums;
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
            SolutionSteps = result.SolutionSteps
        };
    }
}