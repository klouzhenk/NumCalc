import sympy
import numpy as np
from typing import List, Tuple, Callable, Any

def generate_points(f: Callable[[Any], Any], a: float, b: float, steps: int = 100) -> List[Tuple[float, float]]:
    try:
        padding = (b - a) * 0.1
        xs = np.linspace(a - padding, b + padding, steps)

        ys = f(xs)

        valid_mask = np.isfinite(ys)

        return list(zip(xs[valid_mask], ys[valid_mask]))
    except Exception:
        return []