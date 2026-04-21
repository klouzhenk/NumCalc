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
                description="Improved Euler (Heun) method formula",
                latex_formula=r"\tilde{y}_{n+1} = y_n + h \cdot f(x_n, y_n), \quad y_{n+1} = y_n + \frac{h}{2}\bigl(f(x_n, y_n) + f(x_{n+1}, \tilde{y}_{n+1})\bigr)",
                value=""
            )
        ]

        step_index = 2
        max_display = 10
        while x < target_x - 1e-12:
            h = min(step_size, target_x - x)
            k1 = float(f(x, y))
            y_pred = y + h * k1
            k2 = float(f(x + h, y_pred))
            y_next = y + (h / 2) * (k1 + k2)
            x_next = x + h

            if step_index <= max_display + 1:
                latex = (
                    r"\begin{aligned}"
                    r"k_1 &= f(" + f"{x:.6g},\\, {y:.4f}" + r") = " + f"{k1:.4f}" + r" && \text{(slope at start)} \\"
                    r"\tilde{y}_{n+1} &= " + f"{y:.4f}" + r" + " + f"{h:.6g}" + r" \cdot " + f"{k1:.4f}" + r" = " + f"{y_pred:.4f}" + r" && \text{(predictor)} \\"
                    r"k_2 &= f(" + f"{x_next:.6g},\\, {y_pred:.4f}" + r") = " + f"{k2:.4f}" + r" && \text{(predicted slope)} \\"
                    r"y_{" + str(step_index - 1) + r"} &= " + f"{y:.4f}" + r" + \tfrac{" + f"{h:.6g}" + r"}{2}(" + f"{k1:.4f}" + r" + " + f"{k2:.4f}" + r") = " + f"{y_next:.4f}" + r" && \text{(corrector)}"
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
