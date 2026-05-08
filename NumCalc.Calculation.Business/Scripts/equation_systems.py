from typing import List
from eq_systems import cramer, gaussian, fixed_point, seidel

def solve_cramer(equations: List[str], variables: List[str]) -> str:
    return cramer.solve(equations, variables)

def solve_gaussian(equations: List[str], variables: List[str]) -> str:
    return gaussian.solve(equations, variables)

def solve_fixed_point(
    iteration_functions: List[str],
    variables: List[str],
    initial_guess: List[float],
    tolerance: float,
    max_iterations: int
) -> str:
    return fixed_point.solve(iteration_functions, variables, initial_guess, tolerance, max_iterations)

def solve_seidel(
    iteration_functions: List[str],
    variables: List[str],
    initial_guess: List[float],
    tolerance: float,
    max_iterations: int
) -> str:
    return seidel.solve(iteration_functions, variables, initial_guess, tolerance, max_iterations)