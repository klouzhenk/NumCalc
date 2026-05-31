from dataclasses import asdict
import json
import logging
import sympy
import numpy as np
from shared.structures import ResponseEnvelope, SuccessData, FailureData, Point, SolutionStep
from shared.functions import generate_points
from shared.parsing import parse_expression

def solve(expression: str, a: float, b: float, tolerance: float = 0.001) -> str:
    try:
        x_sym = sympy.symbols('x')
        try:
            expr = parse_expression(expression)
        except (Exception,):
            logging.exception("Unexpected error while parsing formula")
            envelope = ResponseEnvelope(failure=FailureData("SYNTAX_ERROR", "Invalid formula syntax"), success=None)
            return json.dumps(asdict(envelope))

        try:
            deriv_expr = sympy.diff(expr, x_sym)
            f = sympy.lambdify(x_sym, expr, modules="numpy")
            df = sympy.lambdify(x_sym, deriv_expr, modules="numpy")
        except Exception as e:
            envelope = ResponseEnvelope(failure=FailureData("DERIVATIVE_ERROR", f"Could not calculate derivative: {str(e)}"), success=None)
            return json.dumps(asdict(envelope))

        try:
            df_a, df_b = float(df(a)), float(df(b))
            max_df = max(abs(df_a), abs(df_b))

            if max_df == 0:
                envelope = ResponseEnvelope(failure=FailureData("ZERO_DERIVATIVE", "Derivative is zero in the interval"), success=None)
                return json.dumps(asdict(envelope))

            mid_val = (a + b) / 2.0
            sign = 1.0 if float(df(mid_val)) > 0 else -1.0

            lam = (1.0 / max_df) * sign

        except Exception as e:
            envelope = ResponseEnvelope(failure=FailureData("EVALUATION_ERROR", f"Error evaluating derivative: {str(e)}"), success=None)
            return json.dumps(asdict(envelope))

        iterations = 0
        max_iterations = 1000
        x_curr = (a + b) / 2.0

        steps_log = []
        steps_log.append(SolutionStep(
            step_index=0,
            description=f"Initialization. Lambda calculated: {lam:.5f}",
            latex_formula=f"x_0 = {x_curr:.5f}",
            value=f"Start point: {x_curr:.5f}"
        ))

        root = x_curr

        while iterations < max_iterations:
            iterations += 1

            f_val = float(f(x_curr))
            x_next = x_curr - lam * f_val

            if abs(x_next) > 1e15 or np.isnan(x_next) or np.isinf(x_next):
                envelope = ResponseEnvelope(failure=FailureData("DIVERGENCE", "Method diverged (values became too large)"), success=None)
                return json.dumps(asdict(envelope))

            delta = abs(x_next - x_curr)

            step_desc = f"Iteration #{iterations}: x = x - λ * f(x)"
            step_latex = f"{x_curr:.5f} - ({lam:.4f}) \\cdot ({f_val:.4f}) = {x_next:.5f}"

            steps_log.append(SolutionStep(
                step_index=iterations,
                description=step_desc,
                latex_formula=f"x_{{{iterations}}} = {step_latex}",
                value=f"Delta: {delta:.6f}"
            ))

            x_curr = x_next
            root = x_curr

            if delta < tolerance:
                steps_log.append(SolutionStep(
                    step_index=iterations,
                    description="Convergence criteria met",
                    latex_formula=f"|x_{{{iterations}}} - x_{{{iterations-1}}}| < {tolerance}",
                    value=f"Root found: {root:.6f}"
                ))
                break

        if iterations >= max_iterations:
            envelope = ResponseEnvelope(failure=FailureData("NO_CONVERGENCE", "Max iterations limit reached"), success=None)
            return json.dumps(asdict(envelope))

        points = generate_points(f, a, b)
        points_objects = [Point(x=p[0], y=p[1]) for p in points]

        envelope = ResponseEnvelope(
            success=SuccessData(root, iterations, points_objects, steps_log),
            failure=None
        )
        return json.dumps(asdict(envelope))

    except Exception as e:
        logging.exception("Global error in simple iterations")
        envelope = ResponseEnvelope(failure=FailureData("UNKNOWN_ERROR", str(e)), success=None)
        return json.dumps(asdict(envelope))