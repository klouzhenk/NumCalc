import json
import numpy as np
from typing import List, Optional
from dataclasses import asdict
from scipy.interpolate import CubicSpline
import sympy
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

        xs = np.array(x_nodes, dtype=float)
        ys = np.array(y_values, dtype=float)

        if len(xs) < 2:
            envelope = InterpolationResponseEnvelope(
                success=None,
                failure=FailureData("INVALID_INPUT", "Cubic spline requires at least 2 nodes")
            )
            return json.dumps(asdict(envelope))

        cs = CubicSpline(xs, ys)
        interpolated_value = float(cs(query_point))

        steps = []
        n_segments = len(xs) - 1
        coeffs = cs.c  # shape (4, n_segments): rows are c3, c2, c1, c0 per segment

        steps.append(SolutionStep(
            step_index=1,
            description=f"Cubic spline built with {n_segments} segment(s) over {len(xs)} nodes",
            latex_formula=f"S(x) \\text{{ — piecewise cubic on }} [{xs[0]:.4f},\\, {xs[-1]:.4f}]",
            value=f"{n_segments} segment(s)"
        ))

        # Find which segment contains the query point
        seg_idx = int(np.searchsorted(xs, query_point, side='right')) - 1
        seg_idx = max(0, min(seg_idx, n_segments - 1))
        c3, c2, c1, c0 = coeffs[:, seg_idx]

        steps.append(SolutionStep(
            step_index=2,
            description=f"Active segment: [{xs[seg_idx]:.4f}, {xs[seg_idx + 1]:.4f}]",
            latex_formula=(
                f"S_{seg_idx}(x) = {c3:.5f}(x - {xs[seg_idx]:.4f})^3 "
                f"+ {c2:.5f}(x - {xs[seg_idx]:.4f})^2 "
                f"+ {c1:.5f}(x - {xs[seg_idx]:.4f}) "
                f"+ {c0:.5f}"
            ),
            value=f"Coefficients: [{c3:.5f}, {c2:.5f}, {c1:.5f}, {c0:.5f}]"
        ))

        steps.append(SolutionStep(
            step_index=3,
            description=f"Evaluating spline at x = {query_point}",
            latex_formula=f"S({query_point}) = {interpolated_value:.6f}",
            value=f"{interpolated_value:.6f}"
        ))

        chart_pts = generate_points(cs, float(xs[0]), float(xs[-1]))
        chart_points = [Point(x=p[0], y=p[1]) for p in chart_pts]

        envelope = InterpolationResponseEnvelope(
            success=InterpolationSuccessData(
                interpolated_value=interpolated_value,
                polynomial_latex=None,
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
