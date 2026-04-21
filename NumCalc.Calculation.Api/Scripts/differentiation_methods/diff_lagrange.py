import json
import sympy
import numpy as np
from typing import List, Optional
from dataclasses import asdict
from shared.structures import DifferentiationResponseEnvelope, DifferentiationSuccessData, FailureData, Point, SolutionStep
from shared.parsing import parse_expression
from shared.functions import generate_points, round_sympy_expression


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
                envelope = DifferentiationResponseEnvelope(
                    success=None,
                    failure=FailureData("SYNTAX_ERROR", "Could not evaluate the function at the given nodes")
                )
                return json.dumps(asdict(envelope))
            y_values = computed

        if y_values is None or len(y_values) != len(x_nodes):
            envelope = DifferentiationResponseEnvelope(
                success=None,
                failure=FailureData("INVALID_INPUT", "y_values must be provided and match the length of x_nodes")
            )
            return json.dumps(asdict(envelope))

        n = len(x_nodes)
        xs = x_nodes[:]
        ys = y_values[:]
        x = sympy.Symbol('x')

        steps = []

        # Build Lagrange polynomial
        basis_polys = []
        for i in range(n):
            Li = sympy.Integer(1)
            for j in range(n):
                if i != j:
                    Li = Li * (x - xs[j]) / (xs[i] - xs[j])
            basis_polys.append(sympy.expand(Li))

        poly = sympy.Integer(0)
        for i in range(n):
            poly = poly + ys[i] * basis_polys[i]

        poly_expanded = sympy.expand(poly)
        poly_latex = sympy.latex(round_sympy_expression(poly_expanded))

        steps.append(SolutionStep(
            step_index=1,
            description="Lagrange interpolation polynomial P(x)",
            latex_formula=f"P(x) = {poly_latex}",
            value=""
        ))

        # Differentiate symbolically
        deriv = sympy.diff(poly_expanded, x)
        deriv_expanded = sympy.expand(deriv)
        deriv_latex = sympy.latex(round_sympy_expression(deriv_expanded))

        steps.append(SolutionStep(
            step_index=2,
            description="Derivative of the Lagrange polynomial P'(x)",
            latex_formula=f"P'(x) = {deriv_latex}",
            value=""
        ))

        # Evaluate at query point
        deriv_numeric = sympy.lambdify(x, deriv_expanded, modules="numpy")
        derivative_value = float(deriv_numeric(query_point))

        steps.append(SolutionStep(
            step_index=3,
            description=f"Evaluating P'(x) at x = {query_point}",
            latex_formula=f"P'({query_point}) = {derivative_value:.8f}",
            value=f"{derivative_value:.8f}"
        ))

        # Chart: plot P(x) over the node range
        poly_numeric = sympy.lambdify(x, poly_expanded, modules="numpy")
        x_min, x_max = min(xs), max(xs)
        chart_pts = generate_points(poly_numeric, x_min, x_max)
        chart_points = [Point(x=float(p[0]), y=float(p[1])) for p in chart_pts]

        envelope = DifferentiationResponseEnvelope(
            success=DifferentiationSuccessData(
                derivative_value=derivative_value,
                polynomial_latex=f"P'(x) = {deriv_latex}",
                chart_points=chart_points,
                solution_steps=steps
            ),
            failure=None
        )
        return json.dumps(asdict(envelope))

    except Exception as e:
        envelope = DifferentiationResponseEnvelope(
            success=None,
            failure=FailureData("UNKNOWN_ERROR", str(e))
        )
        return json.dumps(asdict(envelope))
