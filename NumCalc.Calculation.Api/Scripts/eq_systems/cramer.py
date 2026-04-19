import json
import sympy
from typing import List
from dataclasses import asdict
from shared.structures import SystemResponseEnvelope, SystemSuccessData, FailureData, SolutionStep

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
                parsed_eqs.append(sympy.sympify(f"{left} - ({right})"))
            else:
                parsed_eqs.append(sympy.sympify(eq))

        system_matrix = sympy.linear_eq_to_matrix(parsed_eqs, *symbols)
        A, B = system_matrix

        det_A = A.det()

        if det_A == 0:
            failure = FailureData(
                code="ZERO_DETERMINANT",
                message="The main determinant is zero. The system does not have a unique solution."
            )
            return json.dumps(asdict(SystemResponseEnvelope(success=None, failure=failure)))

        det_steps = [SolutionStep(
            step_index=1,
            description="Calculating the main determinant",
            latex_formula=f"\\Delta = {sympy.latex(A)} = {det_A}",
            value=f"\\Delta = {det_A}"
        )]

        roots = []
        aux_dets = []

        for i in range(len(variables)):
            Ai = A.copy()
            Ai[:, i] = B
            det_Ai = Ai.det()
            aux_dets.append(det_Ai)
            root_val = float(det_Ai / det_A)
            roots.append(root_val)

            det_steps.append(SolutionStep(
                step_index=i + 2,
                description=f"Calculating auxiliary determinant for variable {variables[i]}",
                latex_formula=f"\\Delta_{{{variables[i]}}} = {sympy.latex(Ai)} = {det_Ai}",
                value=f"\\Delta_{{{i+1}}} = {det_Ai}"
            ))

        value_steps = []
        for i, (variable, det_Ai, root_val) in enumerate(zip(variables, aux_dets, roots)):
            fmt = int(root_val) if root_val == int(root_val) else round(root_val, 8)
            value_steps.append(SolutionStep(
                step_index=len(variables) + i + 2,
                description=f"Finding the value of {variable}",
                latex_formula=f"{variable} = \\frac{{\\Delta_{{{variable}}}}}{{\\Delta}} = \\frac{{{det_Ai}}}{{{det_A}}}",
                value=f"{variable} = {fmt}"
            ))

        steps = det_steps + value_steps

        success = SystemSuccessData(roots=roots, solution_steps=steps)
        return json.dumps(asdict(SystemResponseEnvelope(success=success, failure=None)))

    except Exception as e:
        failure = FailureData(code="CALCULATION_ERROR", message=str(e))
        return json.dumps(asdict(SystemResponseEnvelope(success=None, failure=failure)))