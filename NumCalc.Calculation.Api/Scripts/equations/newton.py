from dataclasses import asdict
import json
import sympy
import numpy as np
from shared.structures import ResponseEnvelope, SuccessData, FailureData, Point, SolutionStep
from shared.functions import generate_points
from shared.parsing import parse_expression

def solve(expression: str, x0: float, tolerance: float = 0.001) -> str:
    try:
        x = sympy.symbols('x')
        try:
            expr = parse_expression(expression)
        except (Exception,):
            envelope = ResponseEnvelope(failure=FailureData("SYNTAX_ERROR", "Invalid formula syntax"), success=None)
            return json.dumps(asdict(envelope))

        deriv = sympy.diff(expr, x)

        steps_log = []

        steps_log.append(SolutionStep(
            step_index=0,
            description="Analytic differentiation",
            latex_formula=f"f'(x) = {sympy.latex(deriv)}",
            value="Derivative was found"
        ))

        f = sympy.lambdify(x, expr, modules="numpy")
        f_prime = sympy.lambdify(x, deriv, modules="numpy")

        iterations = 0
        root = 0.0
        max_iterations = 100
        curr_x = x0
        converged = False

        steps_log.append(SolutionStep(
            step_index=0,
            description="Initial approximation",
            latex_formula=f"x_0 = {curr_x}",
            value=f"f(x_0) = {float(f(curr_x)):.5f}"
        ))

        for iteration in range(1, max_iterations + 1):
            f_val = f(curr_x)
            df_val = f_prime(curr_x)

            if df_val == 0:
                break

            next_x = curr_x - f_val / df_val

            iterations += 1
            steps_log.append(SolutionStep(
                step_index=iteration,
                description=f"Iteration {iteration}",
                latex_formula=f"x_{iteration} = {next_x:.5f}",
                value=f"f(x_{iteration}) = {f_val:.5f}"
            ))

            if abs(next_x - curr_x) <= tolerance and abs(f_val) <= tolerance:
                curr_x = next_x
                converged = True
                break

            curr_x = next_x

        root = curr_x

        if not converged:
            envelope = ResponseEnvelope(failure=FailureData("NO_CONVERGENCE", "Newton method did not converge"), success=None)
            return json.dumps(asdict(envelope))

        plot_range = abs(root - x0) * 2 if abs(root - x0) > 1 else 5.0
        points = generate_points(f, root - plot_range, root + plot_range)
        points_objects = [Point(x=p[0], y=p[1]) for p in points]

        envelope = ResponseEnvelope(
            success=SuccessData(root, iterations, points_objects, steps_log),
            failure=None
        )
        return json.dumps(asdict(envelope))

    except Exception as e:
        envelope = ResponseEnvelope(failure=FailureData("UNKNOWN_ERROR", str(e)), success=None)
        return json.dumps(asdict(envelope))