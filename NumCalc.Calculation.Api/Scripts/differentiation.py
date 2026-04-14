from typing import List, Optional
from differentiation import diff_lagrange, finite_diff

def solve_finite_diff(expression: str, x_point: float, h: float, order: int) -> str:
    return finite_diff.solve(expression, x_point, h, order)

def solve_lagrange(
    x_nodes: List[float],
    y_values: Optional[List[float]],
    query_point: float,
    expression: Optional[str] = None
) -> str:
    return diff_lagrange.solve(x_nodes, y_values, query_point, expression)