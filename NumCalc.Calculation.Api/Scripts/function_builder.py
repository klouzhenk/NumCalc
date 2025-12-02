from typing import List, Tuple
import numpy as np
import sympy as sp

def root_finding_points(
        func_expr: str,
        a: float,
        b: float,
        steps: int = 200
) -> List[Tuple[float, float]]:

    x = sp.symbols("x")
    f = sp.lambdify(x, sp.sympify(func_expr), "numpy")

    xs = np.linspace(a, b, steps)
    points = [(float(xv), float(f(xv))) for xv in xs]
    return points