from equation_systems import cramer

def solve_cramer(equations: List[str], variables: List[str]) -> str:
    return cramer.solve(equations, variables)