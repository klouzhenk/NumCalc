import json
import numpy as np
import sympy
from dataclasses import asdict
from shared.structures import OptimizationResponseEnvelope, OptimizationSuccessData, FailureData, Point, SolutionStep
from shared.parsing import parse_expression
from shared.functions import generate_points


def solve(expression: str, lower_bound: float, upper_bound: float, n: int) -> str:
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

        xs = np.linspace(lower_bound, upper_bound, n + 1)
        ys = np.array([float(f(x)) for x in xs])

        idx = int(np.argmin(ys))
        x_star = float(xs[idx])
        f_star = float(ys[idx])
        h = (upper_bound - lower_bound) / n

        steps = [
            SolutionStep(
                step_index=1,
                description="Grid parameters",
                latex_formula=r"h = \frac{b - a}{n},\quad x_i = a + i \cdot h",
                value=f"n = {n}, h = {h:.8f}"
            ),
            SolutionStep(
                step_index=2,
                description="Minimum found",
                latex_formula=r"x^* = \arg\min_{x_i} f(x_i)",
                value=f"x* = {x_star:.8f}, f(x*) = {f_star:.8f}"
            ),
        ]

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
