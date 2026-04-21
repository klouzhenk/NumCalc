from typing import List
from dataclasses import dataclass, field

@dataclass
class Point:
    x: float
    y: float
    z: float | None = None

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
class EquationChartSeries:
    label: str
    points: List[Point]

@dataclass
class SystemSuccessData:
    roots: List[float]
    chart_series: List[EquationChartSeries] | None = None
    solution_steps: List[SolutionStep] = field(default_factory=list)

@dataclass
class SystemResponseEnvelope:
    success: SystemSuccessData | None
    failure: FailureData | None

# Interpolation result
@dataclass
class InterpolationSuccessData:
    interpolated_value: float
    polynomial_latex: str | None
    chart_points: List[Point]
    solution_steps: List[SolutionStep] = field(default_factory=list)

@dataclass
class InterpolationResponseEnvelope:
    success: InterpolationSuccessData | None
    failure: FailureData | None

# Differentiation result
@dataclass
class DifferentiationSuccessData:
    derivative_value: float
    polynomial_latex: str | None
    chart_points: List[Point]
    solution_steps: List[SolutionStep] = field(default_factory=list)

@dataclass
class DifferentiationResponseEnvelope:
    success: DifferentiationSuccessData | None
    failure: FailureData | None

# Optimization result
@dataclass
class OptimizationSuccessData:
    minimum_value: float
    arg_min_x: float | None
    arg_min_point: List[float] | None
    chart_points: List[Point] | None
    solution_steps: List[SolutionStep] = field(default_factory=list)

@dataclass
class OptimizationResponseEnvelope:
    success: OptimizationSuccessData | None
    failure: FailureData | None

# Integration result
@dataclass
class IntegrationSuccessData:
    integral_value: float
    chart_points: List[Point]
    solution_steps: List[SolutionStep] = field(default_factory=list)
    shape_points: List[Point] | None = None

@dataclass
class IntegrationResponseEnvelope:
    success: IntegrationSuccessData | None
    failure: FailureData | None

# ODE result
@dataclass
class OdeSuccessData:
    solution_points: List[Point]
    polynomial_latex: str | None
    solution_steps: List[SolutionStep] = field(default_factory=list)

@dataclass
class OdeResponseEnvelope:
    success: OdeSuccessData | None
    failure: FailureData | None