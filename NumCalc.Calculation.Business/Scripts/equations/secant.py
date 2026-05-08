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
            logging.exception("Error parsing formula")
            envelope = ResponseEnvelope(failure=FailureData("SYNTAX_ERROR", "Invalid formula syntax"), success=None)
            return json.dumps(asdict(envelope))

        f = sympy.lambdify(x_sym, expr, modules="numpy")

        try:
            fa, fb = float(f(a)), float(f(b))
        except Exception as e:
            envelope = ResponseEnvelope(failure=FailureData("EVALUATION_ERROR", str(e)), success=None)
            return json.dumps(asdict(envelope))

        iterations = 0
        max_iterations = 1000

        x0 = a
        x1 = b

        steps_log = []
        steps_log.append(SolutionStep(
            step_index=0,
            description="Initialization",
            latex_formula=f"x_0 = {x0:.5f}, x_1 = {x1:.5f}",
            value=f"f(x0)={fa:.4f}, f(x1)={fb:.4f}"
        ))

        root = None

        while iterations < max_iterations:
            iterations += 1

            f0 = float(f(x0))
            f1 = float(f(x1))

            if abs(f1 - f0) < 1e-15:
                envelope = ResponseEnvelope(failure=FailureData("DIVISION_BY_ZERO", "f(x1) equals f(x0), cannot divide"), success=None)
                return json.dumps(asdict(envelope))

            x_next = x1 - f1 * (x1 - x0) / (f1 - f0)

            delta = abs(x_next - x1)

            step_desc = f"Iteration #{iterations}: Secant formula"
            step_latex = f"x_{{{iterations+1}}} = {x1:.5f} - {f1:.4f} \\cdot \\frac{{{x1:.4f} - {x0:.4f}}}{{{f1:.4f} - {f0:.4f}}} = {x_next:.5f}"

            steps_log.append(SolutionStep(
                step_index=iterations,
                description=step_desc,
                latex_formula=step_latex,
                value=f"x_next: {x_next:.6f}, Delta: {delta:.6f}"
            ))

            x0 = x1
            x1 = x_next
            root = x1

            if delta < tolerance:
                steps_log.append(SolutionStep(
                    step_index=iterations + 1,
                    description="Convergence criteria met",
                    latex_formula=f"|x_{{{iterations+1}}} - x_{{{iterations}}}| < {tolerance}",
                    value=f"Root found: {root:.6f}"
                ))
                break

        if iterations >= max_iterations:
            envelope = ResponseEnvelope(failure=FailureData("NO_CONVERGENCE", "Max iterations reached"), success=None)
            return json.dumps(asdict(envelope))

        points = generate_points(f, a, b)
        points_objects = [Point(x=p[0], y=p[1]) for p in points]

        envelope = ResponseEnvelope(
            success=SuccessData(root, iterations, points_objects, steps_log),
            failure=None
        )
        return json.dumps(asdict(envelope))

    except Exception as e:
        logging.exception("Global error in secant method")
        envelope = ResponseEnvelope(failure=FailureData("UNKNOWN_ERROR", str(e)), success=None)
        return json.dumps(asdict(envelope))