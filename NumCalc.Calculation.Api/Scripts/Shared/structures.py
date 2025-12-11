from typing import List, Tuple
from dataclasses import dataclass

@dataclass
class Point:
    x: float
    y: float

@dataclass
class SuccessData:
    root: float
    iterations: int
    chart_points: List[Point]

@dataclass
class FailureData:
    code: str
    message: str

@dataclass
class ResponseEnvelope:
    success: SuccessData | None
    failure: FailureData | None
