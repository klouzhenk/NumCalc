from integration_methods import rectangle, trapezoid, simpson

def solve_rectangle(expression: str, lower_bound: float, upper_bound: float, n: int) -> str:
    return rectangle.solve(expression, lower_bound, upper_bound, n)

def solve_trapezoid(expression: str, lower_bound: float, upper_bound: float, n: int) -> str:
    return trapezoid.solve(expression, lower_bound, upper_bound, n)

def solve_simpson(expression: str, lower_bound: float, upper_bound: float, n: int) -> str:
    return simpson.solve(expression, lower_bound, upper_bound, n)
