import json
import math
import sympy
from dataclasses import asdict
from shared.structures import OptimizationResponseEnvelope, OptimizationSuccessData, FailureData, Point, SolutionStep
from shared.parsing import parse_expression
from shared.functions import generate_points

_PHI = (math.sqrt(5) - 1) / 2  # ≈ 0.618034


def _fmt(v: float) -> str:
    if v == 0:
        return "0.000000"
    return f"{v:.6f}" if abs(v) >= 0.0001 else f"{v:.4e}"


def solve(expression: str, lower_bound: float, upper_bound: float, tolerance: float, maximize: bool = False) -> str:
    try:
        if lower_bound >= upper_bound:
            envelope = OptimizationResponseEnvelope(
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
            envelope = OptimizationResponseEnvelope(
                success=None,
                failure=FailureData("SYNTAX_ERROR", "Could not evaluate the function at the given point")
            )
            return json.dumps(asdict(envelope))

        a, b = lower_bound, upper_bound
        x1 = b - _PHI * (b - a)
        x2 = a + _PHI * (b - a)
        f1, f2 = float(f(x1)), float(f(x2))

        steps = [
            SolutionStep(
                step_index=1,
                description="Golden ratio constant",
                latex_formula=r"\varphi = \frac{\sqrt{5}-1}{2} \approx 0.618034",
                value=f"Initial x₁ = {_fmt(x1)},  x₂ = {_fmt(x2)}"
            )
        ]

        iteration = 0

        while (b - a) > tolerance:
            iteration += 1

            x1_before, x2_before = x1, x2
            f1_before, f2_before = f1, f2

            if (f1 < f2) if maximize else (f1 > f2):
                op = ">" if not maximize else "<"
                decision = f"a := x₁ = {_fmt(x1_before)}  (keep right subinterval)"
                a = x1
                x1, f1 = x2, f2
                x2 = a + _PHI * (b - a)
                f2 = float(f(x2))
            else:
                op = "≤" if not maximize else "≥"
                decision = f"b := x₂ = {_fmt(x2_before)}  (keep left subinterval)"
                b = x2
                x2, f2 = x1, f1
                x1 = b - _PHI * (b - a)
                f1 = float(f(x1))

            steps.append(SolutionStep(
                step_index=iteration + 1,
                description=f"Iteration {iteration}",
                latex_formula=(
                    rf"x_1 = {x1_before:.6f},\; f(x_1) = {_fmt(f1_before)}"
                    rf"\qquad x_2 = {x2_before:.6f},\; f(x_2) = {_fmt(f2_before)}"
                ),
                value=f"f(x₁) {op} f(x₂)  →  {decision}  |  [a, b] = [{_fmt(a)}, {_fmt(b)}],  |b−a| = {_fmt(b - a)}"
            ))

            if iteration >= 1000:
                break

        x_min = (a + b) / 2.0
        f_min = float(f(x_min))

        steps.append(SolutionStep(
            step_index=iteration + 2,
            description="Result",
            latex_formula=r"x^* = \frac{a + b}{2}",
            value=f"x* = {x_min:.8f},  f(x*) = {f_min:.8f}"
        ))

        chart_points = [Point(x=float(p[0]), y=float(p[1])) for p in generate_points(f, lower_bound, upper_bound)]

        envelope = OptimizationResponseEnvelope(
            success=OptimizationSuccessData(
                minimum_value=f_min,
                arg_min_x=x_min,
                arg_min_point=None,
                chart_points=chart_points,
                solution_steps=steps
            ),
            failure=None
        )
        return json.dumps(asdict(envelope))

    except Exception as e:
        envelope = OptimizationResponseEnvelope(
            success=None,
            failure=FailureData("UNKNOWN_ERROR", str(e))
        )
        return json.dumps(asdict(envelope))
