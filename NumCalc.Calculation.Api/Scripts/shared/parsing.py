import re
import sympy
from latex2sympy2 import latex2sympy

def parse_expression(expression: str):
    expression = re.sub(r'(\w)\s+\(', r'\1(', expression)
    try:
        return sympy.sympify(expression)
    except Exception:
        return latex2sympy(expression)