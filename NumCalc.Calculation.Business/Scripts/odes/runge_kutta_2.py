import json
import sympy
from dataclasses import asdict
from shared.structures import OdeResponseEnvelope, OdeSuccessData, FailureData, Point, SolutionStep
from shared.parsing import parse_expression

def solve(expression: str, initial_x: float, initial_y: float, target_x: float, step_size: float) -> str:
    try:
        if target_x <= initial_x:
            envelope = OdeResponseEnvelope(
                success=None,
                failure=FailureData("RANGE_INVALID", "target_x must be greater than initial_x")
            )
            return json.dumps(asdict(envelope))

        if step_size <= 0:
            envelope = OdeResponseEnvelope(
                success=None,
                failure=FailureData("RANGE_INVALID", "step_size must be positive")
            )
            return json.dumps(asdict(envelope))

        x_sym, y_sym = sympy.symbols('x y')
        expr = parse_expression(expression)
        f = sympy.lambdify([x_sym, y_sym], expr, modules="numpy")

        try:
            _ = float(f(initial_x, initial_y))
        except Exception:
            envelope = OdeResponseEnvelope(
                success=None,
                failure=FailureData("SYNTAX_ERROR", "Could not evaluate f(x, y) at the initial point")
            )
            return json.dumps(asdict(envelope))

        x = initial_x
        y = initial_y
        solution_points = [Point(x=float(x), y=float(y))]
        steps = [
            SolutionStep(
                step_index=1,
                description="Runge-Kutta 2nd order (midpoint) formula",
                latex_formula=r"k_1 = h \cdot f(x_n, y_n), \quad k_2 = h \cdot f\!\left(x_n + \tfrac{h}{2},\, y_n + \tfrac{k_1}{2}\right), \quad y_{n+1} = y_n + k_2",
                value=""
            )
        ]

        step_index = 2
        max_display = 10
        while x < target_x - 1e-12:
            h = min(step_size, target_x - x)
            k1 = h * float(f(x, y))
            k2 = h * float(f(x + h / 2, y + k1 / 2))
            y_next = y + k2
            x_next = x + h

            if step_index <= max_display + 1:
                latex = (
                    r"\begin{aligned}"
                    r"k_1 &= " + f"{k1:.4f}" + r" && \text{(slope at } x_n \text{)} \\"
                    r"k_2 &= " + f"{k2:.4f}" + r" && \text{(slope at midpoint } x_n + h/2 \text{)} \\"
                    r"y_{" + str(step_index - 1) + r"} &= " + f"{y:.4f}" + r" + " + f"{k2:.4f}" + r" = " + f"{y_next:.4f}"
                    r"\end{aligned}"
                )
                steps.append(SolutionStep(
                    step_index=step_index,
                    description=f"Step x = {x:.6g}",
                    latex_formula=latex,
                    value=f"y({x_next:.6g}) ≈ {y_next:.4f}"
                ))

            x = x_next
            y = y_next
            solution_points.append(Point(x=float(x), y=float(y)))
            step_index += 1

        envelope = OdeResponseEnvelope(
            success=OdeSuccessData(
                solution_points=solution_points,
                polynomial_latex=None,
                solution_steps=steps
            ),
            failure=None
        )
        return json.dumps(asdict(envelope))

    except Exception as e:
        envelope = OdeResponseEnvelope(
            success=None,
            failure=FailureData("UNKNOWN_ERROR", str(e))
        )
        return json.dumps(asdict(envelope))
