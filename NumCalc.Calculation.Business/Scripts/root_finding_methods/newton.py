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
        f = sympy.lambdify(x, expr, modules="numpy")
        f_prime = sympy.lambdify(x, deriv, modules="numpy")

        try:
            f_x0 = float(f(x0))
        except Exception as e:
            envelope = ResponseEnvelope(failure=FailureData("EVALUATION_ERROR", str(e)), success=None)
            return json.dumps(asdict(envelope))

        steps_log = []
        steps_log.append(SolutionStep(
            step_index=0,
            description=f"Initialization. Derivative: f'(x) = {sympy.latex(deriv)}",
            latex_formula=f"x_0 = {x0}",
            value=f"f(x_0) = {f_x0:.5f}"
        ))

        iterations = 0
        max_iterations = 100
        curr_x = x0
        converged = False

        for iteration in range(1, max_iterations + 1):
            f_val = float(f(curr_x))
            df_val = float(f_prime(curr_x))

            if df_val == 0:
                envelope = ResponseEnvelope(failure=FailureData("ZERO_DERIVATIVE", f"Derivative became zero at x = {curr_x}"), success=None)
                return json.dumps(asdict(envelope))

            next_x = curr_x - f_val / df_val
            iterations += 1

            try:
                f_next_val = float(f(next_x))
            except (Exception,):
                f_next_val = np.nan

            steps_log.append(SolutionStep(
                step_index=iteration,
                description=f"Iteration #{iteration}: x_n = x_{{{iteration-1}}} - f(x_{{{iteration-1}}})/f'(x_{{{iteration-1}}})",
                latex_formula=f"x_{{{iteration}}} = {next_x:.5f}",
                value=f"f(x_{{{iteration}}}) = {f_next_val:.5f}"
            ))

            if abs(next_x - curr_x) <= tolerance or abs(f_next_val) <= tolerance:
                curr_x = next_x
                converged = True
                break

            curr_x = next_x

        root = curr_x

        if not converged:
            envelope = ResponseEnvelope(failure=FailureData("NO_CONVERGENCE", "Newton method did not converge within maximum iterations"), success=None)
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