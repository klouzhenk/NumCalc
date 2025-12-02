from dataclasses import dataclass, asdict
import json
import logging
import sympy
import numpy as np

@dataclass
class SuccessData:
    root: float
    iterations: int

@dataclass
class FailureData:
    code: str
    message: str

@dataclass
class ResponseEnvelope:
    success: SuccessData | None
    failure: FailureData | None

def solve_dichotomy(expression: str, a: float, b: float, tolerance: float = 0.001) -> str:
    try:
        x = sympy.symbols('x')
        try:
            expr = sympy.sympify(expression)
        except (Exception,):
            logging.exception("Unexpected error while parsing formula")
            envelope = ResponseEnvelope(failure=FailureData("SYNTAX_ERROR", "Invalid formula syntax"), success=None)
            return json.dumps(asdict(envelope))

        f = sympy.lambdify(x, expr, modules="numpy")
        
        try:
            fa, fb = float(f(a)), float(f(b))
        except Exception as e:
            envelope = ResponseEnvelope(failure=FailureData("EVALUATION_ERROR", str(e)), success=None)
            return json.dumps(asdict(envelope))
        
        if fa * fb > 0:
            envelope = ResponseEnvelope(failure=FailureData("RANGE_INVALID", f"Signs are same: f({a})={fa}, f({b})={fb}"), success=None)
            return json.dumps(asdict(envelope))

        iterations = 0
        root = 0.0
        max_iterations = 1000

        curr_a, curr_b = a, b

        while (curr_b - curr_a) / 2 > tolerance and iterations < max_iterations:
            c = (curr_a + curr_b) / 2
            fc = float(f(c))

            if fc == 0:
                root = c
                break

            if float(f(curr_a)) * fc < 0:
                curr_b = c
            else:
                curr_a = c

            iterations += 1
            root = c

        envelope = ResponseEnvelope(
            success=SuccessData(root, iterations),
            failure=None
        )
        return json.dumps(asdict(envelope))

    except Exception as e:
        envelope = ResponseEnvelope(failure=FailureData("UNKNOWN_ERROR", str(e)), success=None)
        return json.dumps(asdict(envelope))