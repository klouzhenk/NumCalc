from typing import List
from dataclasses import dataclass, field

@dataclass
class Point:
    x: float
    y: float

@dataclass
class SolutionStep:
    step_index: int
    description: str
    latex_formula: str
    value: str

@dataclass
class SuccessData:
    root: float
    iterations: int
    chart_points: List[Point]
    solution_steps: List[SolutionStep] = field(default_factory=list)

@dataclass
class FailureData:
    code: str
    message: str

@dataclass
class ResponseEnvelope:
    success: SuccessData | None
    failure: FailureData | None

# Equation system result
@dataclass
class SystemSuccessData:
    roots: List[float]
    solution_steps: List[SolutionStep] = field(default_factory=list)

@dataclass
class SystemResponseEnvelope:
    success: SystemSuccessData | None
    failure: FailureData | None