from typing import List
from optimization_methods import uniform_search, golden_section, gradient_descent

def solve_uniform_search(expression: str, lower_bound: float, upper_bound: float, n: int, maximize: bool) -> str:
    return uniform_search.solve(expression, lower_bound, upper_bound, n, maximize)

def solve_golden_section(expression: str, lower_bound: float, upper_bound: float, tolerance: float, maximize: bool) -> str:
    return golden_section.solve(expression, lower_bound, upper_bound, tolerance, maximize)

def solve_gradient_descent(expression: str, initial_point: List[float], learning_rate: float, tolerance: float, max_iterations: int, maximize: bool) -> str:
    return gradient_descent.solve(expression, initial_point, learning_rate, tolerance, max_iterations, maximize)
