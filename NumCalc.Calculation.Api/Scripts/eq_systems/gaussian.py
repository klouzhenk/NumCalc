import json
import numpy as np
import sympy
from fractions import Fraction
from typing import List
from dataclasses import asdict
from shared.structures import SystemResponseEnvelope, SystemSuccessData, FailureData, SolutionStep


def _to_latex_number(value: float) -> str:
    fraction = Fraction(value).limit_denominator(1000)
    if fraction.denominator == 1:
        return str(fraction.numerator)
    sign = "-" if fraction < 0 else ""
    return f"{sign}\\frac{{{abs(fraction.numerator)}}}{{{abs(fraction.denominator)}}}"


def _augmented_matrix_to_latex(matrix: np.ndarray, num_variables: int) -> str:
    rows = []
    for row_index in range(num_variables):
        coefficients = " & ".join(
            _to_latex_number(matrix[row_index, col]) for col in range(num_variables)
        )
        right_hand_side = _to_latex_number(matrix[row_index, num_variables])
        rows.append(f"{coefficients} & {right_hand_side}")
    column_spec = "c" * num_variables + "|c"
    body = " \\\\ ".join(rows)
    return f"\\left(\\begin{{array}}{{{column_spec}}}{body}\\end{{array}}\\right)"


def solve(equations: List[str], variables: List[str]) -> str:
    try:
        if len(equations) != len(variables):
            failure = FailureData(
                code="INVALID_SYSTEM",
                message=f"Expected {len(variables)} equations, got {len(equations)}."
            )
            return json.dumps(asdict(SystemResponseEnvelope(success=None, failure=failure)))

        symbols = sympy.symbols(variables)
        parsed_equations = []

        for equation in equations:
            if "=" in equation:
                left, right = equation.split("=", 1)
                parsed_equations.append(sympy.sympify(f"{left} - ({right})"))
            else:
                parsed_equations.append(sympy.sympify(equation))

        coefficient_matrix, rhs_vector = sympy.linear_eq_to_matrix(parsed_equations, *symbols)

        num_variables = len(variables)
        augmented_matrix = np.hstack([
            np.array(coefficient_matrix.tolist(), dtype=float),
            np.array(rhs_vector.tolist(), dtype=float).reshape(-1, 1)
        ])

        steps = []
        step_index = 1

        steps.append(SolutionStep(
            step_index=step_index,
            description="Initial Augmented Matrix",
            latex_formula=_augmented_matrix_to_latex(augmented_matrix, num_variables),
            value=""
        ))
        step_index += 1

        # Forward elimination with partial pivoting
        for pivot_col in range(num_variables):
            pivot_row = pivot_col + int(np.argmax(np.abs(augmented_matrix[pivot_col:, pivot_col])))

            if abs(augmented_matrix[pivot_row, pivot_col]) < 1e-12:
                failure = FailureData(
                    code="SINGULAR_MATRIX",
                    message="The matrix is singular. The system has no unique solution."
                )
                return json.dumps(asdict(SystemResponseEnvelope(success=None, failure=failure)))

            if pivot_row != pivot_col:
                augmented_matrix[[pivot_col, pivot_row]] = augmented_matrix[[pivot_row, pivot_col]]
                steps.append(SolutionStep(
                    step_index=step_index,
                    description=f"Row Swap: Column {pivot_col + 1}",
                    latex_formula=f"R_{{{pivot_col+1}}} \\leftrightarrow R_{{{pivot_row+1}}}",
                    value=f"Swap R{pivot_col+1} and R{pivot_row+1}"
                ))
                step_index += 1

            elimination_operations = []
            for target_row in range(pivot_col + 1, num_variables):
                if abs(augmented_matrix[target_row, pivot_col]) < 1e-15:
                    continue
                elimination_factor = augmented_matrix[target_row, pivot_col] / augmented_matrix[pivot_col, pivot_col]
                augmented_matrix[target_row] -= elimination_factor * augmented_matrix[pivot_col]
                elimination_operations.append(
                    f"R_{{{target_row+1}}} \\leftarrow R_{{{target_row+1}}} - "
                    f"{_to_latex_number(elimination_factor)} \\cdot R_{{{pivot_col+1}}}"
                )

            if elimination_operations:
                steps.append(SolutionStep(
                    step_index=step_index,
                    description=f"Forward Elimination: Column {pivot_col + 1}",
                    latex_formula=",\\quad ".join(elimination_operations),
                    value=""
                ))
                step_index += 1

                steps.append(SolutionStep(
                    step_index=step_index,
                    description=f"Matrix after Column {pivot_col + 1}",
                    latex_formula=_augmented_matrix_to_latex(augmented_matrix, num_variables),
                    value=""
                ))
                step_index += 1

        # Back substitution — one step per variable, from last to first
        solution = np.zeros(num_variables)
        for var_index in range(num_variables - 1, -1, -1):
            already_known_sum = float(np.dot(
                augmented_matrix[var_index, var_index+1:num_variables],
                solution[var_index+1:]
            ))
            solution[var_index] = (
                float(augmented_matrix[var_index, num_variables]) - already_known_sum
            ) / float(augmented_matrix[var_index, var_index])

            pivot_coefficient = _to_latex_number(augmented_matrix[var_index, var_index])
            equation_terms = [f"{pivot_coefficient} \\cdot {variables[var_index]}"]

            for known_var_index in range(var_index + 1, num_variables):
                coefficient = float(augmented_matrix[var_index, known_var_index])
                if abs(coefficient) < 1e-12:
                    continue
                known_value = _to_latex_number(solution[known_var_index])
                sign = "+" if coefficient >= 0 else "-"
                equation_terms.append(f"{sign} {_to_latex_number(abs(coefficient))} \\cdot {known_value}")

            equation_left_side = " ".join(equation_terms)
            original_rhs = _to_latex_number(float(augmented_matrix[var_index, num_variables]))
            solution_value = _to_latex_number(solution[var_index])
            display_value = int(solution[var_index]) if solution[var_index] == int(solution[var_index]) else round(solution[var_index], 8)

            steps.append(SolutionStep(
                step_index=step_index,
                description=f"Back-Substitution: {variables[var_index]}",
                latex_formula=f"{equation_left_side} = {original_rhs} \\Rightarrow {variables[var_index]} = {solution_value}",
                value=f"{variables[var_index]} = {display_value}"
            ))
            step_index += 1

        roots = [float(x) for x in solution]
        success = SystemSuccessData(roots=roots, solution_steps=steps)
        return json.dumps(asdict(SystemResponseEnvelope(success=success, failure=None)))

    except Exception as e:
        failure = FailureData(code="CALCULATION_ERROR", message=str(e))
        return json.dumps(asdict(SystemResponseEnvelope(success=None, failure=failure)))
