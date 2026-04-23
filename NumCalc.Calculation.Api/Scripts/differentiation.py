from typing import List, Optional
from differentiation_methods import diff_lagrange, finite_diff_forward, finite_diff_backward, finite_diff_central


def solve_finite_diff_forward(expression: str, x_point: float, h: float, order: int) -> str:
    return finite_diff_forward.solve(expression, x_point, h, order)


def solve_finite_diff_backward(expression: str, x_point: float, h: float, order: int) -> str:
    return finite_diff_backward.solve(expression, x_point, h, order)


def solve_finite_diff_central(expression: str, x_point: float, h: float, order: int) -> str:
    return finite_diff_central.solve(expression, x_point, h, order)


def solve_lagrange(
    x_nodes: List[float],
    y_values: Optional[List[float]],
    query_point: float,
    expression: Optional[str] = None,
    order: int = 1
) -> str:
    return diff_lagrange.solve(x_nodes, y_values, query_point, expression, order)
