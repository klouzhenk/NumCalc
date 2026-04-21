import json
import numpy as np
import sympy
from dataclasses import asdict
from shared.structures import IntegrationResponseEnvelope, IntegrationSuccessData, FailureData, Point, SolutionStep
from shared.parsing import parse_expression
from shared.functions import generate_points

SHAPE_THRESHOLD = 20


def solve(expression: str, lower_bound: float, upper_bound: float, n: int, variant: str) -> str:
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

        match variant.lower():
            case "left":
                value = float(h * np.sum(f(xs[:-1])))
                description = "Left rectangle rule"
                latex = r"I \approx h \sum_{i=0}^{n-1} f(x_i)"
                x0, x1 = float(xs[0]), float(xs[1])
                x_last = float(xs[-2])
                terms_preview = rf"f({x0:.4g}) + f({x1:.4g}) + \cdots + f({x_last:.4g})"
                if n <= SHAPE_THRESHOLD:
                    heights = np.append(f(xs[:-1]), 0.0)
            case "right":
                value = float(h * np.sum(f(xs[1:])))
                description = "Right rectangle rule"
                latex = r"I \approx h \sum_{i=1}^{n} f(x_i)"
                x1, x2 = float(xs[1]), float(xs[2]) if n >= 2 else float(xs[1])
                x_last = float(xs[-1])
                terms_preview = rf"f({x1:.4g}) + f({x2:.4g}) + \cdots + f({x_last:.4g})"
                if n <= SHAPE_THRESHOLD:
                    heights = np.append(f(xs[1:]), 0.0)
            case _:
                mids = (xs[:-1] + xs[1:]) / 2
                value = float(h * np.sum(f(mids)))
                description = "Midpoint rectangle rule"
                latex = r"I \approx h \sum_{i=0}^{n-1} f\!\left(\frac{x_i + x_{i+1}}{2}\right)"
                m0, m1 = float(mids[0]), float(mids[1]) if n >= 2 else float(mids[0])
                m_last = float(mids[-1])
                terms_preview = rf"f({m0:.4g}) + f({m1:.4g}) + \cdots + f({m_last:.4g})"
                if n <= SHAPE_THRESHOLD:
                    heights = np.append(f(mids), 0.0)

        steps = [
            SolutionStep(
                step_index=1,
                description=description,
                latex_formula=latex,
                value=f"I \u2248 {value:.8f}"
            ),
            SolutionStep(
                step_index=2,
                description="Sub-interval calculation",
                latex_formula=rf"h = \frac{{b - a}}{{n}} = \frac{{{upper_bound} - ({lower_bound})}}{{{n}}} = {h:.6g}",
                value=rf"I \approx {h:.6g} \cdot [{terms_preview}]"
            ),
        ]

        chart_pts = generate_points(f, lower_bound, upper_bound)
        chart_points = [Point(x=float(p[0]), y=float(p[1])) for p in chart_pts]

        shape_points = None
        if n <= SHAPE_THRESHOLD:
            shape_points = [Point(x=float(xs[i]), y=float(heights[i])) for i in range(len(xs))]

        envelope = IntegrationResponseEnvelope(
            success=IntegrationSuccessData(
                integral_value=value,
                chart_points=chart_points,
                solution_steps=steps,
                shape_points=shape_points
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
