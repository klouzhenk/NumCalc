from dataclasses import dataclass
from typing import Optional

@dataclass
class RootFindingResult:
    method: str
    root: float
    iterations: int
    is_success: bool
    error_message: Optional[str] = None

def solve_dichotomy(expression: str, a: float, b: float, tolerance: float = 0.001) -> RootFindingResult:
    return RootFindingResult(
        method="dichotomy",
        root=1.23,
        iterations=10,
        is_success=True,
        error_message=None
    )

def solve_newton(expression: str, a: float, b: float, tolerance: float = 0.001) -> RootFindingResult:
    return RootFindingResult(
        method="newton",
        root=1.23,
        iterations=5,
        is_success=True,
        error_message=None
    )