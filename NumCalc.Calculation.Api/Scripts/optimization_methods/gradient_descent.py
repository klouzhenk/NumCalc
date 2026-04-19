import json
import numpy as np
import sympy
from typing import List
from dataclasses import asdict
from shared.structures import OptimizationResponseEnvelope, OptimizationSuccessData, FailureData, Point, SolutionStep
from shared.parsing import parse_expression
from shared.functions import generate_points


def solve(expression: str, initial_point: List[float], learning_rate: float, tolerance: float, max_iterations: int, maximize: bool = False) -> str:
    try:
        expr = parse_expression(expression)
        free_vars = sorted(expr.free_symbols, key=lambda s: str(s))
        n_vars = len(free_vars)

        if n_vars == 0:
            envelope = OptimizationResponseEnvelope(
                success=None,
                failure=FailureData("SYNTAX_ERROR", "Expression has no free variables")
            )
            return json.dumps(asdict(envelope))

        if len(initial_point) != n_vars:
            envelope = OptimizationResponseEnvelope(
                success=None,
                failure=FailureData("INVALID_INPUT",
                    f"Expression has {n_vars} variable(s) ({', '.join(str(v) for v in free_vars)}) "
                    f"but {len(initial_point)} initial point value(s) were provided")
            )
            return json.dumps(asdict(envelope))

        f = sympy.lambdify(free_vars, expr, modules="numpy")
        grad_exprs = [sympy.diff(expr, v) for v in free_vars]
        grad_f = [sympy.lambdify(free_vars, g, modules="numpy") for g in grad_exprs]

        try:
            _ = float(f(*initial_point))
        except Exception:
            envelope = OptimizationResponseEnvelope(
                success=None,
                failure=FailureData("SYNTAX_ERROR", "Could not evaluate the function at the initial point")
            )
            return json.dumps(asdict(envelope))

        point = list(map(float, initial_point))
        steps = []

        # Record every k-th step so we never exceed ~50 steps in the response
        record_every = max(1, max_iterations // 50)

        for i in range(max_iterations):
            f_val = float(f(*point))
            grad_val = [float(g(*point)) for g in grad_f]
            grad_norm = float(np.linalg.norm(grad_val))

            step_latex = r"x_{k+1} = x_k + \alpha \nabla f(x_k)" if maximize else r"x_{k+1} = x_k - \alpha \nabla f(x_k)"
            if i % record_every == 0 or grad_norm < tolerance:
                var_str = ", ".join(f"{str(v)}={p:.6f}" for v, p in zip(free_vars, point))
                steps.append(SolutionStep(
                    step_index=i + 1,
                    description=f"Iteration {i + 1}",
                    latex_formula=step_latex,
                    value=f"{var_str}, f = {f_val:.8f}, |\u2207f| = {grad_norm:.2e}"
                ))

            if grad_norm < tolerance:
                break

            sign = 1 if maximize else -1
            point = [p + sign * learning_rate * g for p, g in zip(point, grad_val)]

        f_star = float(f(*point))

        # Chart only for single-variable case
        chart_points = None
        if n_vars == 1:
            f_1d = sympy.lambdify(free_vars[0], expr, modules="numpy")
            padding = max(5.0, abs(point[0]) * 2)
            chart_pts = generate_points(f_1d, point[0] - padding, point[0] + padding)
            chart_points = [Point(x=float(p[0]), y=float(p[1])) for p in chart_pts]

        envelope = OptimizationResponseEnvelope(
            success=OptimizationSuccessData(
                minimum_value=f_star,
                arg_min_x=None,
                arg_min_point=point,
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
