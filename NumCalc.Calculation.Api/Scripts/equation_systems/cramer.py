import json
import sympy
from typing import List
from dataclasses import asdict
from shared.structures import SystemResponseEnvelope, SystemSuccessData, FailureData, SolutionStep
from shared.parsing import parse_expression

def solve(equations: List[str], variables: List[str]) -> str:
    try:
        symbols = sympy.symbols(variables)
        parsed_eqs = []

        for eq in equations:
            if "=" in eq:
                left, right = eq.split("=")
                parsed_eqs.append(parse_expression(f"{left} - ({right})"))
            else:
                parsed_eqs.append(parse_expression(eq))

        system_matrix = sympy.linear_eq_to_matrix(parsed_eqs, *symbols)
        A, B = system_matrix

        steps = []
        det_A = A.det()

        steps.append(SolutionStep(
            step_index=1,
            description="Calculating the main determinant",
            latex_formula=f"\\Delta = {sympy.latex(A)} = {det_A}",
            value=f"\\Delta = {det_A}"
        ))

        if det_A == 0:
            failure = FailureData(
                code="ZERO_DETERMINANT",
                message="The main determinant is zero. The system does not have a unique solution."
            )
            return json.dumps(asdict(SystemResponseEnvelope(success=None, failure=failure)))

        roots = []

        for i in range(len(variables)):
            Ai = A.copy()
            Ai[:, i] = B
            det_Ai = Ai.det()

            steps.append(SolutionStep(
                step_index=i+2,
                description=f"Calculating auxiliary determinant for variable {variables[i]}",
                latex_formula=f"\\Delta_{{{variables[i]}}} = {sympy.latex(Ai)} = {det_Ai}",
                value=f"\\Delta_{i+1} = {det_Ai}"
            ))

            root_val = float(det_Ai / det_A)
            roots.append(root_val)

            steps.append(SolutionStep(
                step_index=len(variables)+i+2,
                description=f"Finding the value of {variables[i]}",
                latex_formula=f"{variables[i]} = \\frac{{\\Delta_{{{variables[i]}}}}}{{\\Delta}} = \\frac{{{det_Ai}}}{{{det_A}}}",
                value=f"{variables[i]} = {root_val}"
            ))

        success = SystemSuccessData(roots=roots, solution_steps=steps)
        return json.dumps(asdict(SystemResponseEnvelope(success=success, failure=None)))

    except Exception as e:
        failure = FailureData(code="CALCULATION_ERROR", message=str(e))
        return json.dumps(asdict(SystemResponseEnvelope(success=None, failure=failure)))