from dataclasses import asdict
import json
import logging
import sympy
from shared.structures import ResponseEnvelope, SuccessData, FailureData, Point, SolutionStep
from shared.functions import generate_points

def solve(expression: str, a: float, b: float, tolerance: float = 0.001) -> str:
    try:
        x = sympy.symbols('x')
        try:
            expr = sympy.sympify(expression)
        except (Exception,):
            logging.exception("Unexpected error while parsing formula")
            envelope = ResponseEnvelope(failure=FailureData("SYNTAX_ERROR", "Invalid formula syntax"), success=None)
            return json.dumps(asdict(envelope))

        f = sympy.lambdify(x, expr, modules="numpy")

        try:
            fa, fb = float(f(a)), float(f(b))
        except Exception as e:
            envelope = ResponseEnvelope(failure=FailureData("EVALUATION_ERROR", str(e)), success=None)
            return json.dumps(asdict(envelope))

        if fa * fb > 0:
            envelope = ResponseEnvelope(failure=FailureData("RANGE_INVALID", f"Signs are same: f({a})={fa}, f({b})={fb}"), success=None)
            return json.dumps(asdict(envelope))

        iterations = 0
        root = 0.0
        max_iterations = 1000
        curr_a, curr_b = a, b

        steps_log = []
        steps_log.append(SolutionStep(
            step_index=0,
            description="Initial interval",
            latex_formula=f"[{curr_a:.4f}; {curr_b:.4f}]",
            value=f"f(a)={fa:.4f}, f(b)={fb:.4f}"
        ))

        while (curr_b - curr_a) / 2 > tolerance and iterations < max_iterations:
            c = (curr_a + curr_b) / 2
            fc = float(f(c))
            fa = float(f(curr_a))

            iterations += 1

            if abs(fc) < 1e-15:
                root = c

                steps_log.append(SolutionStep(
                    step_index=iterations,
                    description="Root found (value is effectively zero)",
                    latex_formula=f"c = {c:.6f}, f(c) \\approx 0",
                    value=f"Converged at x = {c}"
                ))
                break

            sign_check = fa * fc

            if sign_check < 0:
                next_action = "Root is on the left (b = c)"
                curr_b = c
            else:
                next_action = "Root is on the right (a = c)"
                curr_a = c

            step_desc = f"Iteration #{iterations}: Calculating mid-point. {next_action}"
            step_latex = f"c = {c:.5f}, f(c) = {fc:.5f}"

            steps_log.append(SolutionStep(
                step_index=iterations,
                description=step_desc,
                latex_formula=step_latex,
                value=f"New interval: [{curr_a:.4f}; {curr_b:.4f}]"
            ))

            if fc == 0:
                root = c
                break

            root = c

        points = generate_points(f, a, b)
        points_objects = [Point(x=p[0], y=p[1]) for p in points]

        envelope = ResponseEnvelope(
            success=SuccessData(root, iterations, points_objects, steps_log),
            failure=None
        )
        return json.dumps(asdict(envelope))

    except Exception as e:
        envelope = ResponseEnvelope(failure=FailureData("UNKNOWN_ERROR", str(e)), success=None)
        return json.dumps(asdict(envelope))
