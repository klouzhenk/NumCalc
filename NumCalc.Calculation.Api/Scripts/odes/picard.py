import json
import sympy
from dataclasses import asdict
from shared.structures import OdeResponseEnvelope, OdeSuccessData, FailureData, Point, SolutionStep
from shared.parsing import parse_expression

def solve(expression: str, initial_x: float, initial_y: float, target_x: float, step_size: float, picard_order: int) -> str:
    try:
        if target_x <= initial_x:
            envelope = OdeResponseEnvelope(
                success=None,
                failure=FailureData("RANGE_INVALID", "target_x must be greater than initial_x")
            )
            return json.dumps(asdict(envelope))

        if step_size <= 0:
            envelope = OdeResponseEnvelope(
                success=None,
                failure=FailureData("RANGE_INVALID", "step_size must be positive")
            )
            return json.dumps(asdict(envelope))

        if picard_order < 1 or picard_order > 10:
            envelope = OdeResponseEnvelope(
                success=None,
                failure=FailureData("RANGE_INVALID", "picard_order must be between 1 and 10")
            )
            return json.dumps(asdict(envelope))

        x_sym, y_sym = sympy.symbols('x y')
        t_sym = sympy.Symbol('t')
        expr = parse_expression(expression)

        # Picard successive approximation:
        # phi_0(x) = initial_y
        # phi_{n+1}(x) = initial_y + integral from initial_x to x of f(t, phi_n(t)) dt
        phi = sympy.Float(initial_y)
        x0 = sympy.Float(initial_x)

        steps = [
            SolutionStep(
                step_index=1,
                description="Picard iteration formula",
                latex_formula=r"\varphi_{n+1}(x) = y_0 + \int_{x_0}^{x} f\!\bigl(t,\, \varphi_n(t)\bigr)\, dt",
                value=f"y_0 = {initial_y},\; x_0 = {initial_x}"
            )
        ]

        for i in range(picard_order):
            phi_t = phi.subs(x_sym, t_sym)
            integrand = expr.subs([(x_sym, t_sym), (y_sym, phi_t)])
            try:
                phi = sympy.Float(initial_y) + sympy.integrate(integrand, (t_sym, x0, x_sym))
                phi = sympy.expand(phi)
            except Exception as int_err:
                envelope = OdeResponseEnvelope(
                    success=None,
                    failure=FailureData("UNKNOWN_ERROR", f"Symbolic integration failed at iteration {i + 1}: {str(int_err)}")
                )
                return json.dumps(asdict(envelope))

            steps.append(SolutionStep(
                step_index=i + 2,
                description=f"Approximation φ_{i + 1}(x)",
                latex_formula=r"\varphi_{" + str(i + 1) + r"}(x) = " + sympy.latex(phi),
                value=""
            ))

        # Evaluate polynomial over [initial_x, target_x] to build chart points
        phi_func = sympy.lambdify(x_sym, phi, modules="numpy")
        import numpy as np
        num_points = max(100, int((target_x - initial_x) / max(step_size, 1e-6)))
        num_points = min(num_points, 500)
        xs = np.linspace(initial_x, target_x, num_points)

        solution_points = []
        for xv in xs:
            try:
                yv = float(phi_func(xv))
                if not (abs(yv) < 1e15):  # guard against divergence
                    break
                solution_points.append(Point(x=float(xv), y=yv))
            except Exception:
                break

        if not solution_points:
            envelope = OdeResponseEnvelope(
                success=None,
                failure=FailureData("RANGE_INVALID", "Picard approximation diverges over the given interval; reduce the interval or picard_order")
            )
            return json.dumps(asdict(envelope))

        polynomial_latex = sympy.latex(phi)

        envelope = OdeResponseEnvelope(
            success=OdeSuccessData(
                solution_points=solution_points,
                polynomial_latex=polynomial_latex,
                solution_steps=steps
            ),
            failure=None
        )
        return json.dumps(asdict(envelope))

    except Exception as e:
        envelope = OdeResponseEnvelope(
            success=None,
            failure=FailureData("UNKNOWN_ERROR", str(e))
        )
        return json.dumps(asdict(envelope))
