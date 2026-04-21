import json
import sympy
import numpy as np
from typing import List, Optional
from dataclasses import asdict
from shared.structures import InterpolationResponseEnvelope, InterpolationSuccessData, FailureData, Point, SolutionStep
from shared.parsing import parse_expression
from shared.functions import generate_points, round_sympy_expression


def evaluate_at_nodes(expression: str, x_nodes: List[float]) -> Optional[List[float]]:
    try:
        x = sympy.symbols('x')
        expr = parse_expression(expression)
        f = sympy.lambdify(x, expr, modules="numpy")
        return [float(f(node)) for node in x_nodes]
    except Exception:
        return None


def build_product_form_latex(index: int, x_nodes: List[float]) -> str:
    numerator = " ".join(
        f"(x - {x_nodes[j]:.4f})" for j in range(len(x_nodes)) if j != index
    )
    denominator = " ".join(
        f"({x_nodes[index]:.4f} - {x_nodes[j]:.4f})" for j in range(len(x_nodes)) if j != index
    )
    return f"L_{{{index}}}(x) = \\frac{{{numerator}}}{{{denominator}}}"


def solve(
    x_nodes: List[float],
    y_values: Optional[List[float]],
    query_point: float,
    expression: Optional[str] = None
) -> str:
    try:
        if expression is not None:
            computed_y = evaluate_at_nodes(expression, x_nodes)
            if computed_y is None:
                envelope = InterpolationResponseEnvelope(
                    success=None,
                    failure=FailureData("SYNTAX_ERROR", "Could not evaluate the function at the given nodes")
                )
                return json.dumps(asdict(envelope))
            y_values = computed_y

        if y_values is None or len(y_values) != len(x_nodes):
            envelope = InterpolationResponseEnvelope(
                success=None,
                failure=FailureData("INVALID_INPUT", "y_values must be provided and match the length of x_nodes")
            )
            return json.dumps(asdict(envelope))

        node_count = len(x_nodes)
        x = sympy.Symbol('x')

        steps = []
        basis_polynomials = []

        for i in range(node_count):
            basis_poly = sympy.Integer(1)
            for j in range(node_count):
                if i != j:
                    basis_poly = basis_poly * (x - x_nodes[j]) / (x_nodes[i] - x_nodes[j])

            basis_poly_expanded = sympy.expand(basis_poly)
            basis_polynomials.append(basis_poly_expanded)

            basis_value_at_query = round(float(basis_poly_expanded.subs(x, query_point)), 6)

            steps.append(SolutionStep(
                step_index=i + 1,
                description=f"Basis polynomial L_{i}(x) — product form",
                latex_formula=build_product_form_latex(i, x_nodes),
                value=f"L_{i}({query_point}) = {basis_value_at_query}"
            ))

        # Weighted sum step: P(x) = y0*L0(x) + y1*L1(x) + ...
        weighted_sum_terms = " + ".join(
            f"{round(y_values[i], 6)} \\cdot L_{{{i}}}(x)" for i in range(node_count)
        )
        steps.append(SolutionStep(
            step_index=node_count + 1,
            description="Interpolation polynomial as weighted sum of basis polynomials",
            latex_formula=f"P(x) = {weighted_sum_terms}",
            value=""
        ))

        interpolation_poly = sympy.Integer(0)
        for i in range(node_count):
            interpolation_poly = interpolation_poly + y_values[i] * basis_polynomials[i]

        expanded_poly = sympy.expand(interpolation_poly)
        polynomial_latex = sympy.latex(round_sympy_expression(expanded_poly))

        steps.append(SolutionStep(
            step_index=node_count + 2,
            description="Expanded Lagrange interpolation polynomial",
            latex_formula=f"P(x) = {polynomial_latex}",
            value=""
        ))

        numeric_function = sympy.lambdify(x, expanded_poly, modules="numpy")
        interpolated_value = float(numeric_function(query_point))

        steps.append(SolutionStep(
            step_index=node_count + 3,
            description=f"Evaluating polynomial at x = {query_point}",
            latex_formula=f"P({query_point}) = {round(interpolated_value, 6)}",
            value=f"{round(interpolated_value, 6)}"
        ))

        x_min, x_max = min(x_nodes), max(x_nodes)
        chart_pts = generate_points(numeric_function, x_min, x_max)
        chart_points = [Point(x=p[0], y=p[1]) for p in chart_pts]

        envelope = InterpolationResponseEnvelope(
            success=InterpolationSuccessData(
                interpolated_value=interpolated_value,
                polynomial_latex=polynomial_latex,
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
