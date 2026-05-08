import json
import sympy
import math
from typing import List
from dataclasses import asdict
from shared.structures import SystemResponseEnvelope, SystemSuccessData, FailureData, SolutionStep
from shared.eq_chart import nonlinear_chart_series

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
        parsed_funcs = [sympy.sympify(f) for f in iteration_functions]
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
            x_new = [float(f(*x)) for f in lambdified]

            if any(math.isnan(v) or math.isinf(v) for v in x_new):
                failure = FailureData(
                    code="DIVERGENCE",
                    message=f"The iteration diverged at step {iteration}."
                )
                return json.dumps(asdict(SystemResponseEnvelope(success=None, failure=failure)))

            steps.append(SolutionStep(
                step_index=iteration,
                description="Iteration",
                latex_formula=_format_state(variables, x_new),
                value=_format_state(variables, x_new).replace(",\\ ", ", ")
            ))

            if max(abs(x_new[i] - x[i]) for i in range(len(variables))) < tolerance:
                chart = nonlinear_chart_series(iteration_functions, variables, x_new)
                success = SystemSuccessData(roots=x_new, chart_series=chart, solution_steps=steps)
                return json.dumps(asdict(SystemResponseEnvelope(success=success, failure=None)))

            x = x_new

        failure = FailureData(
            code="MAX_ITERATIONS_REACHED",
            message=f"The method did not converge within {max_iterations} iterations."
        )
        return json.dumps(asdict(SystemResponseEnvelope(success=None, failure=failure)))

    except Exception as e:
        failure = FailureData(code="CALCULATION_ERROR", message=str(e))
        return json.dumps(asdict(SystemResponseEnvelope(success=None, failure=failure)))
