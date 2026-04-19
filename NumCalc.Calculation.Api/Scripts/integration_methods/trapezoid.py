import json
import numpy as np
import sympy
from dataclasses import asdict
from shared.structures import IntegrationResponseEnvelope, IntegrationSuccessData, FailureData, Point, SolutionStep
from shared.parsing import parse_expression
from shared.functions import generate_points


def solve(expression: str, lower_bound: float, upper_bound: float, n: int) -> str:
    try:
        if lower_bound >= upper_bound:
            envelope = IntegrationResponseEnvelope(
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
            envelope = IntegrationResponseEnvelope(
                success=None,
                failure=FailureData("SYNTAX_ERROR", "Could not evaluate the function at the given point")
            )
            return json.dumps(asdict(envelope))

        h = (upper_bound - lower_bound) / n
        xs = np.linspace(lower_bound, upper_bound, n + 1)
        ys = f(xs)

        result = float(h * (ys[0] / 2 + np.sum(ys[1:-1]) + ys[-1] / 2))

        steps = [
            SolutionStep(
                step_index=1,
                description="Composite trapezoidal rule",
                latex_formula=r"I \approx h\left[\frac{f(a)}{2} + \sum_{i=1}^{n-1} f(x_i) + \frac{f(b)}{2}\right]",
                value=f"I \u2248 {result:.8f}"
            ),
            SolutionStep(
                step_index=2,
                description="Parameters",
                latex_formula=r"h = \frac{b - a}{n}",
                value=f"h = ({upper_bound} - {lower_bound}) / {n} = {h:.8f}"
            ),
        ]

        chart_pts = generate_points(f, lower_bound, upper_bound)
        chart_points = [Point(x=float(p[0]), y=float(p[1])) for p in chart_pts]

        envelope = IntegrationResponseEnvelope(
            success=IntegrationSuccessData(
                integral_value=result,
                chart_points=chart_points,
                solution_steps=steps
            ),
            failure=None
        )
        return json.dumps(asdict(envelope))

    except Exception as e:
        envelope = IntegrationResponseEnvelope(
            success=None,
            failure=FailureData("UNKNOWN_ERROR", str(e))
        )
        return json.dumps(asdict(envelope))
