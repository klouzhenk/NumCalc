import json
import sympy
from dataclasses import asdict
from shared.structures import DifferentiationResponseEnvelope, DifferentiationSuccessData, FailureData, Point, SolutionStep
from shared.parsing import parse_expression
from shared.functions import generate_points


def solve(expression: str, x_point: float, h: float, order: int) -> str:
    try:
        if order not in (1, 2):
            return json.dumps(asdict(DifferentiationResponseEnvelope(
                success=None,
                failure=FailureData("INVALID_INPUT", "order must be 1 or 2")
            )))

        x_sym = sympy.symbols('x')
        expr = parse_expression(expression)
        f = sympy.lambdify(x_sym, expr, modules="numpy")

        try:
            _ = float(f(x_point))
        except Exception:
            return json.dumps(asdict(DifferentiationResponseEnvelope(
                success=None,
                failure=FailureData("SYNTAX_ERROR", "Could not evaluate the function at the given point")
            )))

        if order == 1:
            value = (f(x_point + h) - f(x_point)) / h
            latex = r"f'(x) \approx \frac{f(x+h) - f(x)}{h}"
            description = "Forward difference — 1st derivative, accuracy O(h)"
            label = "f'"
        else:
            value = (f(x_point + 2 * h) - 2 * f(x_point + h) + f(x_point)) / (h ** 2)
            latex = r"f''(x) \approx \frac{f(x+2h) - 2f(x+h) + f(x)}{h^2}"
            description = "Forward difference — 2nd derivative, accuracy O(h)"
            label = "f''"

        steps = [SolutionStep(
            step_index=1,
            description=description,
            latex_formula=latex,
            value=f"{label}({x_point}) ≈ {float(value):.8f}"
        )]

        padding = max(3.0, abs(x_point) * 0.5)
        chart_pts = generate_points(f, x_point - padding, x_point + padding)
        chart_points = [Point(x=float(p[0]), y=float(p[1])) for p in chart_pts]

        return json.dumps(asdict(DifferentiationResponseEnvelope(
            success=DifferentiationSuccessData(
                derivative_value=float(value),
                polynomial_latex=None,
                chart_points=chart_points,
                solution_steps=steps
            ),
            failure=None
        )))

    except Exception as e:
        return json.dumps(asdict(DifferentiationResponseEnvelope(
            success=None,
            failure=FailureData("UNKNOWN_ERROR", str(e))
        )))
