import numpy as np
import sympy
from scipy.optimize import brentq
from shared.structures import Point, EquationChartSeries

_RANGE = 10
_N_POINTS = 200


def linear_chart_series(equations, variables, roots):
    """2×2 only. Plots each linear equation as a straight line."""
    if len(variables) != 2:
        return None

    x1_star, x2_star = roots[0], roots[1]
    syms = sympy.symbols(variables)
    series = []

    for i, eq in enumerate(equations):
        if "=" in eq:
            left, right = eq.split("=", 1)
            expr = sympy.sympify(f"({left}) - ({right})")
        else:
            expr = sympy.sympify(eq)

        # Solve for x2 in terms of x1
        try:
            x2_solutions = sympy.solve(expr, syms[1])
            if x2_solutions:
                x2_func = sympy.lambdify(syms[0], x2_solutions[0], modules="numpy")
                pts = [
                    Point(x=float(x1), y=float(x2_func(x1)))
                    for x1 in np.linspace(x1_star - _RANGE, x1_star + _RANGE, 2)
                ]
                series.append(EquationChartSeries(label=f"Eq. {i + 1}: {eq}", points=pts))
                continue
        except Exception:
            pass

        # Fallback: solve for x1 in terms of x2 (vertical-ish lines)
        try:
            x1_solutions = sympy.solve(expr, syms[0])
            if x1_solutions:
                x1_func = sympy.lambdify(syms[1], x1_solutions[0], modules="numpy")
                pts = [
                    Point(x=float(x1_func(x2)), y=float(x2))
                    for x2 in np.linspace(x2_star - _RANGE, x2_star + _RANGE, 2)
                ]
                series.append(EquationChartSeries(label=f"Eq. {i + 1}: {eq}", points=pts))
        except Exception:
            pass

    return series if series else None


def nonlinear_chart_series(iteration_functions, variables, roots):
    """2×2 only. Plots each implicit curve xi = gi(x1, x2) via brentq."""
    if len(variables) != 2:
        return None

    x1_star, x2_star = roots[0], roots[1]
    syms = sympy.symbols(variables)
    lambdified = [
        sympy.lambdify(syms, sympy.sympify(f), modules="numpy")
        for f in iteration_functions
    ]

    series = []
    x1_vals = np.linspace(x1_star - _RANGE, x1_star + _RANGE, _N_POINTS)

    for i, g_func in enumerate(lambdified):
        # Curve: variables[i] - gi(x1, x2) = 0
        # Parametrize by x1, root-find x2 via brentq
        def make_h(gf, var_idx):
            def h(x1_val, x2_val):
                gval = gf(x1_val, x2_val)
                return (x1_val if var_idx == 0 else x2_val) - gval
            return h

        h_i = make_h(g_func, i)

        points = []
        for x1 in x1_vals:
            try:
                x2 = brentq(
                    lambda x2_val: h_i(x1, x2_val),
                    x2_star - _RANGE,
                    x2_star + _RANGE,
                    maxiter=50,
                    full_output=False
                )
                points.append(Point(x=float(x1), y=float(x2)))
            except (ValueError, RuntimeError):
                pass

        if points:
            label = f"{variables[i]} = {iteration_functions[i]}"
            series.append(EquationChartSeries(label=label, points=points))

    return series if series else None
