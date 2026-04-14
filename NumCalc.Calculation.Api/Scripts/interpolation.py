from typing import List, Optional
from interpolation import newton, lagrange, spline


def solve_newton(
    x_nodes: List[float],
    y_values: Optional[List[float]],
    query_point: float,
    expression: Optional[str] = None
) -> str:
    return newton.solve(x_nodes, y_values, query_point, expression)


def solve_lagrange(
    x_nodes: List[float],
    y_values: Optional[List[float]],
    query_point: float,
    expression: Optional[str] = None
) -> str:
    return lagrange.solve(x_nodes, y_values, query_point, expression)


def solve_spline(
    x_nodes: List[float],
    y_values: Optional[List[float]],
    query_point: float,
    expression: Optional[str] = None
) -> str:
    return spline.solve(x_nodes, y_values, query_point, expression)
