import sympy
import numpy as np
from typing import List, Tuple, Callable, Any

_SURFACE_RANGE = 10
_SURFACE_GRID_N = 15


def round_sympy_expression(expr: sympy.Expr, decimal_places: int = 6) -> sympy.Expr:
    return expr.xreplace({
        atom: sympy.Float(round(float(atom), decimal_places))
        for atom in expr.atoms(sympy.Float)
    })


def generate_surface(f: Callable, x_star: float, y_star: float) -> List[Tuple[float, float, float]]:
    """Generates a _SURFACE_GRID_N×_SURFACE_GRID_N scatter grid for f(x, y) centered on (x_star, y_star)."""
    x_vals = np.linspace(x_star - _SURFACE_RANGE, x_star + _SURFACE_RANGE, _SURFACE_GRID_N)
    y_vals = np.linspace(y_star - _SURFACE_RANGE, y_star + _SURFACE_RANGE, _SURFACE_GRID_N)
    points = []
    for x in x_vals:
        for y in y_vals:
            try:
                z = float(f(x, y))
                if np.isfinite(z):
                    points.append((float(x), float(y), z))
            except Exception:
                pass
    return points


def generate_points(f: Callable[[Any], Any], a: float, b: float, steps: int = 100) -> List[Tuple[float, float]]:
    try:
        padding = (b - a) * 0.1
        xs = np.linspace(a - padding, b + padding, steps)

        ys = f(xs)

        valid_mask = np.isfinite(ys)

        return list(zip(xs[valid_mask], ys[valid_mask]))
    except Exception:
        return []