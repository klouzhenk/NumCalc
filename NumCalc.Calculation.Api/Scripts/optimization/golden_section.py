import json
import math
import sympy
from dataclasses import asdict
from shared.structures import OptimizationResponseEnvelope, OptimizationSuccessData, FailureData, Point, SolutionStep
from shared.parsing import parse_expression
from shared.functions import generate_points

_PHI = (math.sqrt(5) - 1) / 2  # golden ratio conjugate ≈ 0.618


def solve(expression: str, lower_bound: float, upper_bound: float, tolerance: float) -> str:
    try:
        if lower_bound >= upper_bound:
            envelope = OptimizationResponseEnvelope(
                success=None,
                failure=FailureData("RANGE_INVALID", "lower_bound must be less than upper_bound")
            )
            return json.dumps(asdict(envelope))

        x_sym = sympy.symbols('x')
        expr = parse_expression(expression)
        f = sympy.lambdify(x_sym, expr, modules="numpy")

        try:
            _ = float(f(lower_bound))
        except Exception:
            envelope = OptimizationResponseEnvelope(
                success=None,
                failure=FailureData("SYNTAX_ERROR", "Could not evaluate the function at the given point")
            )
            return json.dumps(asdict(envelope))

        a, b = lower_bound, upper_bound
        x1 = b - _PHI * (b - a)
        x2 = a + _PHI * (b - a)
        f1, f2 = float(f(x1)), float(f(x2))

        steps = []
        iteration = 0

        while (b - a) > tolerance:
            iteration += 1
            if f1 > f2:
                a = x1
                x1, f1 = x2, f2
                x2 = a + _PHI * (b - a)
                f2 = float(f(x2))
            else:
                b = x2
                x2, f2 = x1, f1
                x1 = b - _PHI * (b - a)
                f1 = float(f(x1))

            steps.append(SolutionStep(
                step_index=iteration,
                description=f"Iteration {iteration}",
                latex_formula=r"[a,b] \leftarrow [a + (1-\varphi)(b-a),\, b] \text{ or } [a,\, b-(1-\varphi)(b-a)]",
                value=f"a = {a:.6f}, b = {b:.6f}, |b-a| = {b - a:.2e}"
            ))

            if iteration >= 1000:
                break

        x_star = (a + b) / 2.0
        f_star = float(f(x_star))

        steps.append(SolutionStep(
            step_index=iteration + 1,
            description="Result",
            latex_formula=r"x^* = \frac{a + b}{2}",
            value=f"x* = {x_star:.8f}, f(x*) = {f_star:.8f}"
        ))

        chart_pts = generate_points(f, lower_bound, upper_bound)
        chart_points = [Point(x=float(p[0]), y=float(p[1])) for p in chart_pts]

        envelope = OptimizationResponseEnvelope(
            success=OptimizationSuccessData(
                minimum_value=f_star,
                arg_min_x=x_star,
                arg_min_point=None,
                chart_points=chart_points,
                solution_steps=steps
            ),
            failure=None
        )
        return json.dumps(asdict(envelope))

    except Exception as e:
        envelope = OptimizationResponseEnvelope(
            success=None,
            failure=FailureData("UNKNOWN_ERROR", str(e))
        )
        return json.dumps(asdict(envelope))
