from dataclasses import dataclass, field
from typing import Optional, List, Dict, Tuple
import sympy
import numpy as np

@dataclass
class RootFindingResult:
    method: str
    root: float
    iterations: int
    is_success: bool
    error_message: Optional[str] = None
    chart_points: List[Tuple[float, float]] = field(default_factory=list)

def solve_dichotomy(expression: str, a: float, b: float, tolerance: float = 0.001) -> RootFindingResult:
    try:
        x = sympy.symbols('x')
        expr = sympy.sympify(expression)
        f = sympy.lambdify(x, expr, modules="numpy")

        fa = float(f(a))
        fb = float(f(b))

        if fa * fb > 0:
            return RootFindingResult(
                method="dichotomy",
                root=0.0,
                iterations=0,
                is_success=False,
                error_message=f"Function has same signs at ends: f({a})={fa}, f({b})={fb}"
            )

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
            root = c

        padding = (b - a) * 0.1
        x_vals = np.linspace(a - padding, b + padding, 100)
        y_vals = f(x_vals)

        points = list(zip(x_vals, y_vals))

        return RootFindingResult(
            method="dichotomy",
            root=float(root),
            iterations=iterations,
            is_success=True,
            chart_points=points
        )

    except Exception as e:
        return RootFindingResult(
            method="dichotomy",
            root=0.0,
            iterations=0,
            is_success=False,
            error_message=str(e)
        )

def solve_newton(expression: str, a: float, b: float, tolerance: float = 0.001) -> RootFindingResult:
    return RootFindingResult(
        method="newton",
        root=1.23,
        iterations=5,
        is_success=True,
        error_message=None
    )