import json
import numpy as np
from typing import List, Optional
from dataclasses import asdict
from scipy.interpolate import CubicSpline
import sympy
from shared.structures import InterpolationResponseEnvelope, InterpolationSuccessData, FailureData, Point, SolutionStep
from shared.parsing import parse_expression
from shared.functions import generate_points

_NOISE_THRESHOLD = 1e-10


def suppress_noise(value: float) -> float:
    return 0.0 if abs(value) < _NOISE_THRESHOLD else value


def format_coefficient(value: float) -> str:
    return f"{suppress_noise(value):.5f}"


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

        x_array = np.array(x_nodes, dtype=float)
        y_array = np.array(y_values, dtype=float)

        if len(x_array) < 2:
            envelope = InterpolationResponseEnvelope(
                success=None,
                failure=FailureData("INVALID_INPUT", "Cubic spline requires at least 2 nodes")
            )
            return json.dumps(asdict(envelope))

        spline = CubicSpline(x_array, y_array)
        interpolated_value = float(spline(query_point))

        steps = []
        segment_count = len(x_array) - 1
        # spline.c shape: (4, segment_count) — rows are coeffs of (x-x_i)^3, ^2, ^1, ^0
        segment_coefficients = spline.c

        steps.append(SolutionStep(
            step_index=1,
            description=f"Cubic spline built with {segment_count} segment(s) over {len(x_array)} nodes",
            latex_formula=f"S(x) \\text{{ — piecewise cubic on }} [{x_array[0]:.4f},\\, {x_array[-1]:.4f}]",
            value=f"{segment_count} segment(s)"
        ))

        # Second derivatives (moments) at each node — result of solving the tridiagonal system
        second_derivatives = spline.derivative(2)(x_array)
        moment_latex_parts = [f"m_{{{i}}} = {format_coefficient(float(m))}" for i, m in enumerate(second_derivatives)]
        moments_latex = ",\\quad ".join(moment_latex_parts)

        steps.append(SolutionStep(
            step_index=2,
            description="Tridiagonal system for second derivatives (moments mᵢ = S''(xᵢ))",
            latex_formula=(
                "h_{i-1}m_{i-1} + 2(h_{i-1}+h_i)m_i + h_i m_{i+1} = "
                "6\\!\\left(\\frac{y_{i+1}-y_i}{h_i} - \\frac{y_i-y_{i-1}}{h_{i-1}}\\right)"
            ),
            value=f"Solved moments: {moments_latex}"
        ))

        steps.append(SolutionStep(
            step_index=3,
            description="Computed moments at nodes",
            latex_formula=moments_latex,
            value=f"[{', '.join(format_coefficient(float(m)) for m in second_derivatives)}]"
        ))

        # Find which segment contains the query point
        segment_index = int(np.searchsorted(x_array, query_point, side='right')) - 1
        segment_index = max(0, min(segment_index, segment_count - 1))

        # SciPy stores coefficients as [cubic, quadratic, linear, constant]; remap to classic a,b,c,d
        coeff_a = suppress_noise(float(segment_coefficients[3, segment_index]))
        coeff_b = suppress_noise(float(segment_coefficients[2, segment_index]))
        coeff_c = suppress_noise(float(segment_coefficients[1, segment_index]))
        coeff_d = suppress_noise(float(segment_coefficients[0, segment_index]))
        segment_start = x_array[segment_index]

        steps.append(SolutionStep(
            step_index=4,
            description=(
                f"Active segment: [{segment_start:.4f}, {x_array[segment_index + 1]:.4f}], "
                f"where xᵢ = {segment_start:.4f} is the left endpoint"
            ),
            latex_formula=(
                f"S_{{{segment_index}}}(x) = {format_coefficient(coeff_a)} "
                f"+ {format_coefficient(coeff_b)}(x - {segment_start:.4f}) "
                f"+ {format_coefficient(coeff_c)}(x - {segment_start:.4f})^2 "
                f"+ {format_coefficient(coeff_d)}(x - {segment_start:.4f})^3"
            ),
            value=f"a={format_coefficient(coeff_a)}, b={format_coefficient(coeff_b)}, c={format_coefficient(coeff_c)}, d={format_coefficient(coeff_d)}"
        ))

        steps.append(SolutionStep(
            step_index=5,
            description=f"Evaluating spline at x = {query_point}",
            latex_formula=f"S({query_point}) = {interpolated_value:.6f}",
            value=f"{interpolated_value:.6f}"
        ))

        chart_pts = generate_points(spline, float(x_array[0]), float(x_array[-1]))
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
