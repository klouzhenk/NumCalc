import json
import sympy
import math
from typing import List
from dataclasses import asdict
from shared.structures import SystemResponseEnvelope, SystemSuccessData, FailureData, SolutionStep
from shared.parsing import parse_expression

def _format_state(variables, values):
    return ",\\ ".join([f"{variables[i]} = {values[i]:.6g}" for i in range(len(variables))])

def solve(
    iteration_functions: List[str],
    variables: List[str],
    initial_guess: List[float],
    tolerance: float = 1e-6,
    max_iterations: int = 500
) -> str:
    try:
        if len(iteration_functions) != len(variables):
            failure = FailureData(
                code="INVALID_SYSTEM",
                message=f"Expected {len(variables)} iteration functions, got {len(iteration_functions)}."
            )
            return json.dumps(asdict(SystemResponseEnvelope(success=None, failure=failure)))

        if len(initial_guess) != len(variables):
            failure = FailureData(
                code="INVALID_SYSTEM",
                message=f"Expected {len(variables)} initial guess values, got {len(initial_guess)}."
            )
            return json.dumps(asdict(SystemResponseEnvelope(success=None, failure=failure)))

        symbols = sympy.symbols(variables)
        parsed_funcs = [parse_expression(f) for f in iteration_functions]
        lambdified = [sympy.lambdify(symbols, f, modules="numpy") for f in parsed_funcs]

        steps = []
        x = list(initial_guess)

        steps.append(SolutionStep(
            step_index=0,
            description="InitialGuess",
            latex_formula=_format_state(variables, initial_guess),
            value=_format_state(variables, initial_guess).replace(",\\ ", ", ")
        ))

        for iteration in range(1, max_iterations + 1):
            x_old = list(x)

            # Key difference from fixed-point: update x in-place so each
            # variable immediately uses the latest values of previous variables
            for i in range(len(variables)):
                x[i] = float(lambdified[i](*x))

                if math.isnan(x[i]) or math.isinf(x[i]) or abs(x[i]) > 1e15:
                    failure = FailureData(
                        code="DIVERGENCE",
                        message=f"The iteration diverged at step {iteration}."
                    )
                    return json.dumps(asdict(SystemResponseEnvelope(success=None, failure=failure)))

            steps.append(SolutionStep(
                step_index=iteration,
                description="Iteration",
                latex_formula=_format_state(variables, x),
                value=_format_state(variables, x).replace(",\\ ", ", ")
            ))

            if max(abs(x[i] - x_old[i]) for i in range(len(variables))) < tolerance:
                success = SystemSuccessData(roots=list(x), solution_steps=steps)
                return json.dumps(asdict(SystemResponseEnvelope(success=success, failure=None)))

        failure = FailureData(
            code="MAX_ITERATIONS_REACHED",
            message=f"The method did not converge within {max_iterations} iterations."
        )
        return json.dumps(asdict(SystemResponseEnvelope(success=None, failure=failure)))

    except Exception as e:
        failure = FailureData(code="CALCULATION_ERROR", message=str(e))
        return json.dumps(asdict(SystemResponseEnvelope(success=None, failure=failure)))
