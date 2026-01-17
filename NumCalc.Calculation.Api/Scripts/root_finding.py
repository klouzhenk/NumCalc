from equations import dichotomy, newton, simple_iterations, secant, combined

def solve_dichotomy(expr: str, a: float, b: float, tol: float) -> str:
    return dichotomy.solve(expr, a, b, tol)

def solve_newton(expr: str, x0: float, tol: float) -> str:
    return newton.solve(expr, x0, tol)

def solve_simple_iterations(expr: str, a: float, b: float, tol: float) -> str:
    return simple_iterations.solve(expr, a, b, tol)

def solve_secant(expr: str, a: float, b: float, tol: float) -> str:
    return secant.solve(expr, a, b, tol)
    
def solve_combined(expr: str, a: float, b: float, tol: float) -> str:
    return combined.solve(expr, a, b, tol)