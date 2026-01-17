from dataclasses import asdict
import json
import logging
import sympy
import numpy as np
from shared.structures import ResponseEnvelope, SuccessData, FailureData, Point, SolutionStep
from shared.functions import generate_points

def solve(expression: str, a: float, b: float, tolerance: float = 0.001) -> str:
    try:
        x_sym = sympy.symbols('x')
        try:
            expr = parse_expression(expression)
        except (Exception,):
            logging.exception("Error parsing formula")
            envelope = ResponseEnvelope(failure=FailureData("SYNTAX_ERROR", "Invalid formula syntax"), success=None)
            return json.dumps(asdict(envelope))

        f_prime = sympy.diff(expr, x_sym)
        f_double_prime = sympy.diff(f_prime, x_sym)

        f = sympy.lambdify(x_sym, expr, modules="numpy")
        df = sympy.lambdify(x_sym, f_prime, modules="numpy")
        ddf = sympy.lambdify(x_sym, f_double_prime, modules="numpy")

        try:
            fa, fb = float(f(a)), float(f(b))
        except Exception as e:
            envelope = ResponseEnvelope(failure=FailureData("EVALUATION_ERROR", str(e)), success=None)
            return json.dumps(asdict(envelope))

        if fa * fb > 0:
            envelope = ResponseEnvelope(failure=FailureData("RANGE_INVALID", f"Signs are same: f({a})={fa:.4f}, f({b})={fb:.4f}"), success=None)
            return json.dumps(asdict(envelope))

        iterations = 0
        max_iterations = 1000
        curr_a, curr_b = a, b

        steps_log = []
        steps_log.append(SolutionStep(
            step_index=0,
            description="Initialization",
            latex_formula=f"a_0 = {curr_a:.5f}, b_0 = {curr_b:.5f}",
            value=f"Interval: [{curr_a:.5f}; {curr_b:.5f}]"
        ))

        root = None

        while iterations < max_iterations:
            iterations += 1

            fa = float(f(curr_a))
            fb = float(f(curr_b))

            try:
                mid_point = (curr_a + curr_b) / 2
                ddf_val = float(ddf(mid_point))
            except:
                ddf_val = 1

            if fa * ddf_val > 0:
                method_a = "Newton"
                method_b = "Chord"

                dfa = float(df(curr_a))
                if abs(dfa) < 1e-15:
                    envelope = ResponseEnvelope(failure=FailureData("ZERO_DERIVATIVE", "Derivative is zero at 'a'"), success=None)
                    return json.dumps(asdict(envelope))

                next_a = curr_a - fa / dfa

                next_b = curr_b - fb * (curr_b - curr_a) / (fb - fa)

            else:
                method_a = "Chord"
                method_b = "Newton"

                if abs(fb - fa) < 1e-15:
                    envelope = ResponseEnvelope(failure=FailureData("DIVISION_BY_ZERO", "f(b) equals f(a)"), success=None)
                    return json.dumps(asdict(envelope))

                next_a = curr_a - fa * (curr_b - curr_a) / (fb - fa)

                dfb = float(df(curr_b))
                if abs(dfb) < 1e-15:
                    envelope = ResponseEnvelope(failure=FailureData("ZERO_DERIVATIVE", "Derivative is zero at 'b'"), success=None)
                    return json.dumps(asdict(envelope))

                next_b = curr_b - fb / dfb

            if next_a < a or next_a > b: next_a = (curr_a + curr_b) / 2
            if next_b < a or next_b > b: next_b = (curr_a + curr_b) / 2

            if next_a > next_b:
                next_a, next_b = next_b, next_a

            delta = abs(next_b - next_a)
            mid_approx = (next_a + next_b) / 2

            step_desc = f"Iter #{iterations}: a->{method_a}, b->{method_b}"
            step_latex = f"a_{{{iterations}}}={next_a:.5f}, b_{{{iterations}}}={next_b:.5f}"

            steps_log.append(SolutionStep(
                step_index=iterations,
                description=step_desc,
                latex_formula=step_latex,
                value=f"New Interval: [{next_a:.5f}; {next_b:.5f}], Delta: {delta:.6f}"
            ))

            curr_a = next_a
            curr_b = next_b
            root = mid_approx

            if delta < tolerance:
                steps_log.append(SolutionStep(
                    step_index=iterations + 1,
                    description="Convergence criteria met",
                    latex_formula=f"|b_{{{iterations}}} - a_{{{iterations}}}| < {tolerance}",
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
        logging.exception("Global error in combined method")
        envelope = ResponseEnvelope(failure=FailureData("UNKNOWN_ERROR", str(e)), success=None)
        return json.dumps(asdict(envelope))