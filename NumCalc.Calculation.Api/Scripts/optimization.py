from typing import List
from optimization import uniform_search, golden_section, gradient_descent

def solve_uniform_search(expression: str, lower_bound: float, upper_bound: float, n: int) -> str:
    return uniform_search.solve(expression, lower_bound, upper_bound, n)

def solve_golden_section(expression: str, lower_bound: float, upper_bound: float, tolerance: float) -> str:
    return golden_section.solve(expression, lower_bound, upper_bound, tolerance)

def solve_gradient_descent(expression: str, initial_point: List[float], learning_rate: float, tolerance: float, max_iterations: int) -> str:
    return gradient_descent.solve(expression, initial_point, learning_rate, tolerance, max_iterations)
