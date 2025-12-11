from typing import List, Tuple
import numpy as np
import sympy as sp
from plotting import generate_points

def root_finding_points(
        func_expr: str,
        a: float,
        b: float,
        steps: int = 200
) -> List[Tuple[float, float]]:

    x = sp.symbols("x")
    f = sp.lambdify(x, sp.sympify(func_expr), "numpy")

    return generate_points(f, a, b, steps)