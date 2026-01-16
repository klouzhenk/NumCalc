from equations import dichotomy, newton

def solve_dichotomy(expr: str, a: float, b: float, tol: float) -> str:
    return dichotomy.solve(expr, a, b, tol)

def solve_newton(expr: str, x0: float, tol: float) -> str:
    return newton.solve(expr, x0, tol)
