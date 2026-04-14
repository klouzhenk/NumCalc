using CSnakes.Runtime;
using NumCalc.Calculation.Api.Entities;
using NumCalc.Calculation.Api.Entities.EquationSystems;
using NumCalc.Calculation.Api.Exceptions;
using NumCalc.Calculation.Api.Services.Interfaces;
using NumCalc.Calculation.Api.Utils;
using NumCalc.Shared.Enums;
using NumCalc.Shared.EquationsSystems.Requests;
using NumCalc.Shared.EquationsSystems.Responses;

namespace NumCalc.Calculation.Api.Services.Implementations;

public class EquationsSystemService(IPythonEnvironment env) : IEquationsSystemService
{
    public SystemSolvingResponse SolveCramer(SystemSolvingRequest request)
    {
        if (request.Equations == null || request.Equations.Count == 0)
            throw new CustomException(NumCalcErrorCode.SyntaxError, "The entered equations list is empty");

        var equationSystemSolver = env.EquationSystems();

        var jsonEnvelope = equationSystemSolver.SolveCramer(request.Equations, request.Variables);
        
        var result = jsonEnvelope.UnwrapOrThrow<SystemSolvingData>();

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

        var equationSystemSolver = env.EquationSystems();

        var jsonEnvelope = equationSystemSolver.SolveGaussian(request.Equations, request.Variables);

        var result = jsonEnvelope.UnwrapOrThrow<SystemSolvingData>();

        return new SystemSolvingResponse
        {
            Roots = result.Roots,
            SolutionSteps = result.SolutionSteps
        };
    }
}