import json
import sympy
import numpy as np
from typing import List, Optional
from dataclasses import asdict
from shared.structures import DifferentiationResponseEnvelope, DifferentiationSuccessData, FailureData, Point, SolutionStep
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


def solve(
    x_nodes: List[float],
    y_values: Optional[List[float]],
    query_point: float,
    expression: Optional[str] = None,
    order: int = 1
) -> str:
    try:
        if order not in (1, 2):
            envelope = DifferentiationResponseEnvelope(
                success=None,
                failure=FailureData("INVALID_INPUT", "order must be 1 or 2")
            )
            return json.dumps(asdict(envelope))

        if expression is not None:
            computed_y = evaluate_at_nodes(expression, x_nodes)
            if computed_y is None:
                envelope = DifferentiationResponseEnvelope(
                    success=None,
                    failure=FailureData("SYNTAX_ERROR", "Could not evaluate the function at the given nodes")
                )
                return json.dumps(asdict(envelope))
            y_values = computed_y

        if y_values is None or len(y_values) != len(x_nodes):
            envelope = DifferentiationResponseEnvelope(
                success=None,
                failure=FailureData("INVALID_INPUT", "y_values must be provided and match the length of x_nodes")
            )
            return json.dumps(asdict(envelope))

        x = sympy.Symbol('x')
        node_count = len(x_nodes)

        basis_polynomials = []
        for i in range(node_count):
            basis_poly = sympy.Integer(1)
            for j in range(node_count):
                if i != j:
                    basis_poly = basis_poly * (x - x_nodes[j]) / (x_nodes[i] - x_nodes[j])
            basis_polynomials.append(sympy.expand(basis_poly))

        interpolation_poly = sympy.Integer(0)
        for i in range(node_count):
            interpolation_poly = interpolation_poly + y_values[i] * basis_polynomials[i]

        poly_expanded = sympy.expand(interpolation_poly)
        poly_latex = sympy.latex(round_sympy_expression(poly_expanded))

        steps = []
        steps.append(SolutionStep(
            step_index=1,
            description="Lagrange interpolation polynomial P(x)",
            latex_formula=f"P(x) = {poly_latex}",
            value=""
        ))

        first_deriv = sympy.expand(sympy.diff(poly_expanded, x))
        first_deriv_latex = sympy.latex(round_sympy_expression(first_deriv))

        steps.append(SolutionStep(
            step_index=2,
            description="First derivative P'(x)",
            latex_formula=f"P'(x) = {first_deriv_latex}",
            value=""
        ))

        if order == 2:
            second_deriv = sympy.expand(sympy.diff(first_deriv, x))
            second_deriv_latex = sympy.latex(round_sympy_expression(second_deriv))

            steps.append(SolutionStep(
                step_index=3,
                description="Second derivative P''(x)",
                latex_formula=f"P''(x) = {second_deriv_latex}",
                value=""
            ))

            target_deriv = second_deriv
            target_deriv_latex = second_deriv_latex
            result_label = "P''"
            polynomial_latex = f"P''(x) = {second_deriv_latex}"
        else:
            target_deriv = first_deriv
            target_deriv_latex = first_deriv_latex
            result_label = "P'"
            polynomial_latex = f"P'(x) = {first_deriv_latex}"

        deriv_numeric = sympy.lambdify(x, target_deriv, modules="numpy")
        derivative_value = float(deriv_numeric(query_point))

        steps.append(SolutionStep(
            step_index=order + 2,
            description=f"Evaluating {result_label}(x) at x = {query_point}",
            latex_formula=f"{result_label}({query_point}) = {round(derivative_value, 6)}",
            value=f"{round(derivative_value, 6)}"
        ))

        poly_numeric = sympy.lambdify(x, poly_expanded, modules="numpy")
        x_min, x_max = min(x_nodes), max(x_nodes)
        chart_pts = generate_points(poly_numeric, x_min, x_max)
        chart_points = [Point(x=float(p[0]), y=float(p[1])) for p in chart_pts]

        envelope = DifferentiationResponseEnvelope(
            success=DifferentiationSuccessData(
                derivative_value=derivative_value,
                polynomial_latex=polynomial_latex,
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
