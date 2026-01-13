from dataclasses import dataclass, asdict
import json
import logging
import sympy
import numpy as np
from Shared.structures import ResponseEnvelope, SuccessData, FailureData, Point, SolutionStep
from plotting import generate_points

def solve_dichotomy(expression: str, a: float, b: float, tolerance: float = 0.001) -> str:
    try:
        x = sympy.symbols('x')
        try:
            expr = sympy.sympify(expression)
        except (Exception,):
            logging.exception("Unexpected error while parsing formula")
            envelope = ResponseEnvelope(failure=FailureData("SYNTAX_ERROR", "Invalid formula syntax"), success=None)
            return json.dumps(asdict(envelope))

        f = sympy.lambdify(x, expr, modules="numpy")
        
        try:
            fa, fb = float(f(a)), float(f(b))
        except Exception as e:
            envelope = ResponseEnvelope(failure=FailureData("EVALUATION_ERROR", str(e)), success=None)
            return json.dumps(asdict(envelope))
        
        if fa * fb > 0:
            envelope = ResponseEnvelope(failure=FailureData("RANGE_INVALID", f"Signs are same: f({a})={fa}, f({b})={fb}"), success=None)
            return json.dumps(asdict(envelope))

        iterations = 0
        root = 0.0
        max_iterations = 1000

        curr_a, curr_b = a, b

        while (curr_b - curr_a) / 2 > tolerance and iterations < max_iterations:
            c = (curr_a + curr_b) / 2
            fc = float(f(c))

            if fc == 0:
                root = c
                break

            if float(f(curr_a)) * fc < 0:
                curr_b = c
            else:
                curr_a = c

            iterations += 1
            root = c\
            
        points = generate_points(f, a, b)
        points_objects = [Point(x=p[0], y=p[1]) for p in points]
        
        envelope = ResponseEnvelope(
            success=SuccessData(root, iterations, points_objects),
            failure=None
        )
        return json.dumps(asdict(envelope))

    except Exception as e:
        envelope = ResponseEnvelope(failure=FailureData("UNKNOWN_ERROR", str(e)), success=None)
        return json.dumps(asdict(envelope))



def solve_newton(expression: str, x0: float, tolerance: float = 0.001) -> str:
    try:
        x = sympy.symbols('x')
        try:
            expr = sympy.sympify(expression)
        except (Exception,):
            envelope = ResponseEnvelope(failure=FailureData("SYNTAX_ERROR", "Invalid formula syntax"), success=None)
            return json.dumps(asdict(envelope))

        deriv = sympy.diff(expr, x)

        steps_log = []

        steps_log.append(SolutionStep(
            step_index=0,
            description="Analytic differentiation",
            latex_formula=f"f'(x) = {sympy.latex(deriv)}",
            value="Derivative was found"
        ))

        f = sympy.lambdify(x, expr, modules="numpy")
        df = sympy.lambdify(x, deriv, modules="numpy")

        iterations = 0
        root = 0.0
        max_iterations = 100
        curr_x = x0
        converged = False

        steps_log.append(SolutionStep(
            step_index=0,
            description="Initial approximation",
            latex_formula=f"x_0 = {curr_x}",
            value=f"f(x_0) = {float(f(curr_x)):.5f}"
        ))

        f, f_prime = prepare_functions(expression)

        for iteration in range(1, max_iterations + 1):
            f_val = f(curr_x)
            df_val = f_prime(curr_x)
        
            if df_val == 0:
                break
        
            next_x = curr_x - f_val / df_val

            iterations += 1
            steps_log.append(SolutionStep(
                step_index=iteration,
                description=f"Iteration {iteration}",
                latex_formula=f"x_{iteration} = {next_x:.5f}",
                value=f"f(x_{iteration}) = {f_val:.5f}"
            ))
        
            if abs(next_x - curr_x) <= tolerance and abs(f_val) <= tolerance:
                curr_x = next_x
                converged = True
                break
        
            curr_x = next_x
        
        root = curr_x

        if not converged:
            envelope = ResponseEnvelope(failure=FailureData(("NO_CONVERGENCE", "Newton method did not converge")), success=None)
            return json.dumps(asdict(envelope))

        plot_range = abs(root - x0) * 2 if abs(root - x0) > 1 else 5.0
        points = generate_points(f, root - plot_range, root + plot_range)
        points_objects = [Point(x=p[0], y=p[1]) for p in points]

        envelope = ResponseEnvelope(
            success=SuccessData(root, iterations, points_objects, steps_log),
            failure=None
        )
        return json.dumps(asdict(envelope))

    except Exception as e:
        envelope = ResponseEnvelope(failure=FailureData("UNKNOWN_ERROR", str(e)), success=None)
        return json.dumps(asdict(envelope))



def prepare_functions(function_str: str):
    x = sympy.symbols('x')

    try:
        expr = sympy.parse_expr(function_str)
    except Exception:
        raise ValueError("Invalid function expression")

    expr_prime = sympy.diff(expr, x)

    f = sympy.lambdify(x, expr, modules="math")
    f_prime = sympy.lambdify(x, expr_prime, modules="math")

    return f, f_prime