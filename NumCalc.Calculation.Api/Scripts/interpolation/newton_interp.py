import json
import sympy
import numpy as np
from typing import List, Optional
from dataclasses import asdict
from shared.structures import InterpolationResponseEnvelope, InterpolationSuccessData, FailureData, Point, SolutionStep
from shared.parsing import parse_expression
from shared.functions import generate_points


def _evaluate_y_values(expression: str, x_nodes: List[float]) -> Optional[List[float]]:
    try:
        x = sympy.symbols('x')
        expr = parse_expression(expression)
        f = sympy.lambdify(x, expr, modules="numpy")
        return [float(f(xi)) for xi in x_nodes]
    except Exception:
        return None


def solve(
    x_nodes: List[float],
    y_values: Optional[List[float]],
    query_point: float,
    expression: Optional[str] = None
) -> str:
    try:
        if expression is not None:
            computed = _evaluate_y_values(expression, x_nodes)
            if computed is None:
                envelope = InterpolationResponseEnvelope(
                    success=None,
                    failure=FailureData("SYNTAX_ERROR", "Could not evaluate the function at the given nodes")
                )
                return json.dumps(asdict(envelope))
            y_values = computed

        if y_values is None or len(y_values) != len(x_nodes):
            envelope = InterpolationResponseEnvelope(
                success=None,
                failure=FailureData("INVALID_INPUT", "y_values must be provided and match the length of x_nodes")
            )
            return json.dumps(asdict(envelope))

        n = len(x_nodes)
        xs = x_nodes[:]
        ys = y_values[:]

        # Build divided differences table
        dd = [ys[:]]
        for order in range(1, n):
            prev = dd[-1]
            curr = []
            for i in range(n - order):
                curr.append((prev[i + 1] - prev[i]) / (xs[i + order] - xs[i]))
            dd.append(curr)

        coeffs = [dd[k][0] for k in range(n)]

        steps = []
        steps.append(SolutionStep(
            step_index=1,
            description="Divided differences table (first column = coefficients)",
            latex_formula="\\begin{array}{" + "c" * (n + 1) + "}" +
                " & ".join([f"x_{i}" for i in range(n)]) + " \\\\ \\hline " +
                " \\\\ ".join([" & ".join([f"{v:.4f}" for v in row]) for row in dd]) +
                "\\end{array}",
            value=f"Coefficients: {[round(c, 5) for c in coeffs]}"
        ))

        # Build symbolic polynomial
        x = sympy.Symbol('x')
        poly = sympy.Integer(0)
        basis = sympy.Integer(1)
        for k in range(n):
            poly = poly + coeffs[k] * basis
            if k < n - 1:
                basis = basis * (x - xs[k])

        poly_expanded = sympy.expand(poly)
        poly_latex = sympy.latex(poly_expanded)

        steps.append(SolutionStep(
            step_index=2,
            description="Newton interpolation polynomial",
            latex_formula=f"P(x) = {poly_latex}",
            value=""
        ))

        f_numeric = sympy.lambdify(x, poly_expanded, modules="numpy")
        interpolated_value = float(f_numeric(query_point))

        steps.append(SolutionStep(
            step_index=3,
            description=f"Evaluating polynomial at x = {query_point}",
            latex_formula=f"P({query_point}) = {interpolated_value:.6f}",
            value=f"{interpolated_value:.6f}"
        ))

        x_min, x_max = min(xs), max(xs)
        chart_pts = generate_points(f_numeric, x_min, x_max)
        chart_points = [Point(x=p[0], y=p[1]) for p in chart_pts]

        envelope = InterpolationResponseEnvelope(
            success=InterpolationSuccessData(
                interpolated_value=interpolated_value,
                polynomial_latex=poly_latex,
                chart_points=chart_points,
                solution_steps=steps
            ),
            failure=None
        )
        return json.dumps(asdict(envelope))

    except Exception as e:
        envelope = InterpolationResponseEnvelope(
            success=None,
            failure=FailureData("UNKNOWN_ERROR", str(e))
        )
        return json.dumps(asdict(envelope))
