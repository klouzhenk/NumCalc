import json
import numpy as np
import sympy
from fractions import Fraction
from typing import List
from dataclasses import asdict
from shared.structures import SystemResponseEnvelope, SystemSuccessData, FailureData, SolutionStep
from shared.parsing import parse_expression

def _factor_latex(value: float) -> str:
    frac = Fraction(value).limit_denominator(1000)
    if frac.denominator == 1:
        return str(frac.numerator)
    sign = "-" if frac < 0 else ""
    return f"{sign}\\frac{{{abs(frac.numerator)}}}{{{frac.denominator}}}"

def solve(equations: List[str], variables: List[str]) -> str:
    try:
        if len(equations) != len(variables):
            failure = FailureData(
                code="INVALID_SYSTEM",
                message=f"Expected {len(variables)} equations, got {len(equations)}."
            )
            return json.dumps(asdict(SystemResponseEnvelope(success=None, failure=failure)))
    
        symbols = sympy.symbols(variables)
        parsed_eqs = []

        for eq in equations:
            if "=" in eq:
                left, right = eq.split("=", 1)
                parsed_eqs.append(parse_expression(f"{left} - ({right})"))
            else:
                parsed_eqs.append(parse_expression(eq))

        system_matrix = sympy.linear_eq_to_matrix(parsed_eqs, *symbols)
        A_sym, B_sym = system_matrix

        n = len(variables)
        aug = np.hstack([
            np.array(A_sym.tolist(), dtype=float),
            np.array(B_sym.tolist(), dtype=float).reshape(-1, 1)
        ])

        steps = []
        step_idx = 1

        aug_sym_initial = sympy.Matrix(A_sym.tolist()).row_join(B_sym)
        aug_latex = sympy.latex(aug_sym_initial)
        steps.append(SolutionStep(
            step_index=step_idx,
            description="InitialAugmentedMatrix",
            latex_formula=f"[A|B] = {aug_latex}",
            value=""
        ))
        step_idx += 1

        # Forward elimination with partial pivoting
        for col in range(n):
            max_row = col + int(np.argmax(np.abs(aug[col:, col])))

            if abs(aug[max_row, col]) < 1e-12:
                failure = FailureData(
                    code="SINGULAR_MATRIX",
                    message="The matrix is singular. The system has no unique solution."
                )
                return json.dumps(asdict(SystemResponseEnvelope(success=None, failure=failure)))

            if max_row != col:
                aug[[col, max_row]] = aug[[max_row, col]]
                steps.append(SolutionStep(
                    step_index=step_idx,
                    description="SwapRows",
                    latex_formula=f"R_{{{col+1}}} \\leftrightarrow R_{{{max_row+1}}}",
                    value=f"Swap R{col+1} and R{max_row+1}"
                ))
                step_idx += 1

            for row in range(col + 1, n):
                if abs(aug[row, col]) < 1e-15:
                    continue
                factor = aug[row, col] / aug[col, col]
                aug[row] -= factor * aug[col]

                steps.append(SolutionStep(
                    step_index=step_idx,
                    description="EliminationStep",
                    latex_formula=f"R_{{{row+1}}} \\leftarrow R_{{{row+1}}} - {_factor_latex(factor)} \\cdot R_{{{col+1}}}",
                    value=f"R{row+1} = R{row+1} - {factor:.4g} * R{col+1}"
                ))
                step_idx += 1

        # Back substitution
        solution = np.zeros(n)
        for i in range(n - 1, -1, -1):
            solution[i] = (aug[i, n] - np.dot(aug[i, i+1:n], solution[i+1:])) / aug[i, i]

        solution_latex = ",\\ ".join([
            f"{variables[i]} = {_factor_latex(solution[i])}"
            for i in range(n)
        ])
        steps.append(SolutionStep(
            step_index=step_idx,
            description="BackSubstitution",
            latex_formula=solution_latex,
            value=", ".join([f"{variables[i]} = {solution[i]:.6g}" for i in range(n)])
        ))

        roots = [float(x) for x in solution]
        success = SystemSuccessData(roots=roots, solution_steps=steps)
        return json.dumps(asdict(SystemResponseEnvelope(success=success, failure=None)))

    except Exception as e:
        failure = FailureData(code="CALCULATION_ERROR", message=str(e))
        return json.dumps(asdict(SystemResponseEnvelope(success=None, failure=failure)))
