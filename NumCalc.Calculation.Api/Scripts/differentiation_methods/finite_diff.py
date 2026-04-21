import json
import sympy
import numpy as np
from dataclasses import asdict
from shared.structures import DifferentiationResponseEnvelope, DifferentiationSuccessData, FailureData, Point, SolutionStep
from shared.parsing import parse_expression
from shared.functions import generate_points


def solve(expression: str, x_point: float, h: float, order: int) -> str:
    try:
        if order not in (1, 2):
            envelope = DifferentiationResponseEnvelope(
                success=None,
                failure=FailureData("INVALID_INPUT", "order must be 1 or 2")
            )
            return json.dumps(asdict(envelope))

        x_sym = sympy.symbols('x')
        expr = parse_expression(expression)
        f = sympy.lambdify(x_sym, expr, modules="numpy")

        try:
            # Probe to catch syntax / evaluation errors early
            _ = float(f(x_point))
        except Exception:
            envelope = DifferentiationResponseEnvelope(
                success=None,
                failure=FailureData("SYNTAX_ERROR", "Could not evaluate the function at the given point")
            )
            return json.dumps(asdict(envelope))

        steps = []

        if order == 1:
            fwd = (f(x_point + h) - f(x_point)) / h
            bwd = (f(x_point) - f(x_point - h)) / h
            cen = (f(x_point + h) - f(x_point - h)) / (2 * h)

            steps.append(SolutionStep(
                step_index=1,
                description="Forward difference — 1st derivative, accuracy O(h)",
                latex_formula=r"f'(x) \approx \frac{f(x+h) - f(x)}{h}",
                value=f"f'({x_point}) \u2248 {float(fwd):.8f}"
            ))
            steps.append(SolutionStep(
                step_index=1,
                description="Backward difference — 1st derivative, accuracy O(h)",
                latex_formula=r"f'(x) \approx \frac{f(x) - f(x-h)}{h}",
                value=f"f'({x_point}) \u2248 {float(bwd):.8f}"
            ))
            steps.append(SolutionStep(
                step_index=1,
                description="Central difference — 1st derivative, accuracy O(h²), most accurate",
                latex_formula=r"f'(x) \approx \frac{f(x+h) - f(x-h)}{2h}",
                value=f"f'({x_point}) \u2248 {float(cen):.8f}"
            ))

            derivative_value = float(cen)

        else:  # order == 2
            fwd = (f(x_point + 2*h) - 2*f(x_point + h) + f(x_point)) / (h ** 2)
            bwd = (f(x_point) - 2*f(x_point - h) + f(x_point - 2*h)) / (h ** 2)
            cen = (f(x_point + h) - 2*f(x_point) + f(x_point - h)) / (h ** 2)

            steps.append(SolutionStep(
                step_index=1,
                description="Forward difference — 2nd derivative, accuracy O(h)",
                latex_formula=r"f''(x) \approx \frac{f(x+2h) - 2f(x+h) + f(x)}{h^2}",
                value=f"f''({x_point}) \u2248 {float(fwd):.8f}"
            ))
            steps.append(SolutionStep(
                step_index=1,
                description="Backward difference — 2nd derivative, accuracy O(h)",
                latex_formula=r"f''(x) \approx \frac{f(x) - 2f(x-h) + f(x-2h)}{h^2}",
                value=f"f''({x_point}) \u2248 {float(bwd):.8f}"
            ))
            steps.append(SolutionStep(
                step_index=1,
                description="Central difference — 2nd derivative, accuracy O(h²), most accurate",
                latex_formula=r"f''(x) \approx \frac{f(x+h) - 2f(x) + f(x-h)}{h^2}",
                value=f"f''({x_point}) \u2248 {float(cen):.8f}"
            ))

            derivative_value = float(cen)

        padding = max(3.0, abs(x_point) * 0.5)
        chart_pts = generate_points(f, x_point - padding, x_point + padding)
        chart_points = [Point(x=float(p[0]), y=float(p[1])) for p in chart_pts]

        envelope = DifferentiationResponseEnvelope(
            success=DifferentiationSuccessData(
                derivative_value=derivative_value,
                polynomial_latex=None,
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
