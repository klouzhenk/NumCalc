from typing import List
from equation_systems import cramer, gaussian

def solve_cramer(equations: List[str], variables: List[str]) -> str:
    return cramer.solve(equations, variables)

def solve_gaussian(equations: List[str], variables: List[str]) -> str:
    return gaussian.solve(equations, variables)