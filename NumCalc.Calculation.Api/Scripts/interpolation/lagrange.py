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
        x = sympy.Symbol('x')

        steps = []
        basis_polys = []

        for i in range(n):
            Li = sympy.Integer(1)
            for j in range(n):
                if i != j:
                    Li = Li * (x - xs[j]) / (xs[i] - xs[j])
            Li_expanded = sympy.expand(Li)
            basis_polys.append(Li_expanded)

            steps.append(SolutionStep(
                step_index=i + 1,
                description=f"Basis polynomial L_{i}(x)",
                latex_formula=f"L_{i}(x) = {sympy.latex(Li_expanded)}",
                value=f"L_{i}({query_point}) = {float(Li_expanded.subs(x, query_point)):.6f}"
            ))

        poly = sympy.Integer(0)
        for i in range(n):
            poly = poly + ys[i] * basis_polys[i]

        poly_expanded = sympy.expand(poly)
        poly_latex = sympy.latex(poly_expanded)

        steps.append(SolutionStep(
            step_index=n + 1,
            description="Lagrange interpolation polynomial",
            latex_formula=f"P(x) = {poly_latex}",
            value=""
        ))

        f_numeric = sympy.lambdify(x, poly_expanded, modules="numpy")
        interpolated_value = float(f_numeric(query_point))

        steps.append(SolutionStep(
            step_index=n + 2,
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
