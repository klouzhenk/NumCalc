import sympy
from latex2sympy2 import latex2sympy

def parse_expression(expression: str):
    try:
        return latex2sympy(expression)
    except Exception:
        return sympy.sympify(expression)