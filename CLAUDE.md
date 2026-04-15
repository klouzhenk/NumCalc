# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

NumCalc is a multi-platform numerical methods application built on .NET 9.0. It solves equations (root finding) and equation systems using numerical algorithms implemented in Python, exposed via a C# REST API, and consumed by a Blazor web UI.

## Build & Run Commands

```bash
# Build entire solution
dotnet build NumCalc.sln

# Run the Calculation API (port 5229)
dotnet run --project NumCalc.Calculation.Api

# Run the Web UI
dotnet run --project NumCalc.UI.Web
```

**Prerequisites:** .NET 9.0 SDK, Python with `sympy`, `numpy`, `scipy`, `mpmath` (see `requirements.txt`). A `.venv/` virtual environment must exist at the repo root for CSnakes to work.

There are no test projects yet.

## Architecture

The solution has 7 projects organized in three layers:

```
NumCalc.UI.Web (Blazor Server)
    └── HTTP → NumCalc.Calculation.Api (ASP.NET Core REST API)
                    └── CSnakes → Scripts/ (Python numerical implementations)

NumCalc.Shared          — DTOs and contracts shared across all projects
NumCalc.UI.Shared       — Reusable Blazor components and HTTP service abstractions
NumCalc.UI.MAUI         — Mobile UI (in progress)
NumCalc.User.API        — Placeholder, not functional yet
NumCalc.Core            — Minimal, mostly unused
```

### Calculation API (`NumCalc.Calculation.Api`)

- **Controllers:** `RootFindingController` (5 methods + comparison endpoint), `EquationsSystemsController` (Cramer, Gaussian, Fixed-point, Seidel), `InterpolationController` (Newton, Lagrange, Spline), `DifferentiationController` (finite-diff, Lagrange derivative), `IntegrationController` (rectangle, trapezoid, Simpson)
- **Services:** `IRootFindingService` / `IEquationsSystemService` / `IInterpolationService` / `IDifferentiationService` / `IIntegrationService` — call into Python via CSnakes
- **Middleware:** `GlobalExceptionHandler` (RFC 7807 Problem Details), Serilog request logging
- **Startup:** `PythonWarmupService` (IHostedService) pre-loads the Python runtime to avoid first-call latency
- Swagger/OpenAPI enabled in development at `/swagger`

### Python Scripts (`Scripts/`)

Numerical logic lives entirely in Python. C# services call these scripts through CSnakes Runtime; Python returns JSON strings that C# deserializes.

```
Scripts/
  equations/          — Root finding algorithms
    dichotomy.py      — Bisection method
    newton.py         — Newton-Raphson (tangent)
    simple_iterations.py
    secant.py
    combined.py       — Brent's hybrid method
  equation_systems/
    cramer.py         — Cramer's rule for linear systems
    gaussian.py       — Gaussian elimination with partial pivoting
    fixed_point.py    — Fixed-point (Jacobi-style) iteration for non-linear systems
    seidel.py         — Gauss-Seidel iteration for non-linear systems
  interpolation/
    newton_interp.py        — Newton divided differences polynomial
    lagrange.py             — Lagrange basis polynomial
    spline.py               — Cubic spline (SciPy)
  differentiation/
    finite_diff.py    — Forward, backward, central finite differences (1st and 2nd order)
    diff_lagrange.py  — Derivative via Lagrange interpolation polynomial (symbolic diff)
  integration/
    rectangle.py      — Left, right, midpoint rectangle rules (all 3 variants in one call)
    trapezoid.py      — Composite trapezoidal rule
    simpson.py        — Composite Simpson's 1/3 rule (auto-corrects odd n to even)
  shared/
    functions.py      — Function parsing/evaluation
    parsing.py
    structures.py     — Shared data structures
```

Top-level dispatcher scripts (CSnakes entry points): `root_finding.py`, `equation_systems.py`, `interpolation.py`, `differentiation.py`, `integration.py`, `warming_up.py`.

**CSnakes naming rule:** Sub-module filenames must be unique across the entire `Scripts/` tree (CSnakes generates proxy hint names from filename only, ignoring folder path). A duplicate filename anywhere causes the entire generator to fail. Also, a top-level `foo.py` and a `foo/` subfolder must not coexist — the `foo/__init__.py` collides with `foo.py`. Sub-folders do not need `__init__.py` (Python 3 namespace packages).

### Shared DTOs (`NumCalc.Shared`)

- Request/response classes for root finding, equation systems, interpolation, differentiation, and integration
- `Point` (x, y), `SolutionStep` (iteration step with LaTeX formula)
- Enums: `RootFindingMethod`, `ErrorCodes` (SyntaxError, RangeInvalid, etc.), `InterpolationInputMode` (Function | RawData), `DifferentiationInputMode` (Function | RawData), `IntegrationInputMode` (Function | RawData)
- `ResponseEnvelope<T>` — wraps success/failure from Python → C# boundary
- `SystemSolvingRequest` — for linear methods (Cramer, Gaussian): `Equations`, `Variables`
- `NonLinearSystemRequest` — for iterative methods (Fixed-point, Seidel): `IterationFunctions`, `Variables`, `InitialGuess`, `Tolerance`, `MaxIterations`
- `LinearIterativeSystemRequest` — reserved for future linear iterative methods
- `InterpolationRequest` — `Mode`, `FunctionExpression?`, `XNodes`, `YValues?`, `QueryPoint`
- `InterpolationResponse` — `InterpolatedValue`, `PolynomialLatex?` (null for Spline), `ChartData`, `SolutionSteps`, `ExecutionTimeMs`
- `DifferentiationRequest` — `Mode`, `FunctionExpression?`, `XNodes?`, `YValues?`, `QueryPoint`, `StepSize` (default 0.001), `DerivativeOrder` (1 or 2)
- `DifferentiationResponse` — `DerivativeValue`, `PolynomialLatex?` (Lagrange only), `ChartData`, `SolutionSteps`, `ExecutionTimeMs`
- `IntegrationRequest` — `Mode`, `FunctionExpression?`, `LowerBound`, `UpperBound`, `Intervals` (default 100)
- `IntegrationResponse` — `IntegralValue`, `ChartData`, `SolutionSteps`, `ExecutionTimeMs`

### UI Shared Library (`NumCalc.UI.Shared`)

- `BaseApiService` — abstract HTTP client base; subclasses implement specific endpoints
- Blazor components: modal dialogs, image cropper (Cropper.Blazor integration)
- Localization resources: `Localization.resx` (English) + `Localization.uk.resx` (Ukrainian)

### Web UI (`NumCalc.UI.Web`)

- Server-side Blazor with interactive server rendering
- Razor components for root finding input, results display, and equation system solving
- Localization: supports `en` and `uk` cultures via `RequestLocalizationOptions`
- Communicates with the Calculation API via typed HTTP clients (registered as `BaseApiService` subclasses)

## Key Patterns

- **Python ↔ C# boundary:** Python scripts return JSON strings → C# deserializes into strongly-typed DTOs via `ResponseEnvelope<T>`. Add new numerical methods by adding a Python script and a corresponding service method + controller endpoint.
- **Error handling:** All exceptions surface through `GlobalExceptionHandler`; numerical errors use typed `ErrorCodes` rather than raw exceptions.
- **Localization:** All user-visible strings go in `NumCalc.UI.Shared/Localization/Localization.resx` (and `.uk.resx`). Do not hard-code display text in components.
- **Shared UI components:** New reusable Blazor components belong in `NumCalc.UI.Shared`, not in `NumCalc.UI.Web`, so they can be reused by MAUI.

## Current State (CRITICAL)

The system is partially implemented.

Currently working:
- Root finding methods (Python + API + UI)
- Equation systems — fully implemented end-to-end (Python + API + HTTP client + Blazor UI):
  - Cramer's rule
  - Gaussian elimination (with partial pivoting)
  - Fixed-point iteration
  - Gauss-Seidel iteration
- Interpolation — fully implemented end-to-end (Python + API + HTTP client + Blazor UI):
  - Newton interpolation polynomial
  - Lagrange interpolation polynomial
  - Cubic spline (SciPy)
  - Supports both Function mode (f(x) + x-nodes) and Raw Data mode (x[], y[])
- Numerical differentiation — fully implemented end-to-end (Python + API + HTTP client + Blazor UI):
  - Finite differences: forward, backward, central (1st and 2nd order)
  - Lagrange derivative (symbolic differentiation of the interpolation polynomial)
  - Chart renders f(x) curve + tangent line at x*
  - Variant dropdown (Forward/Backward/Central) filters which step is displayed
- Numerical integration — fully implemented end-to-end (Python + API + HTTP client + Blazor UI):
  - Rectangle rule: left, right, midpoint (single endpoint; variant dropdown filters displayed step)
  - Trapezoidal rule
  - Simpson's 1/3 rule (auto-corrects odd n to even)
  - Function mode only (f(x) + bounds + n intervals)
- Python integration via CSnakes
- Blazor UI for root finding, equation systems, interpolation, differentiation, and integration

Not implemented yet:
- Optimization
- Differential equations (ODE)
- Full-featured backend for users/history
- Complete MAUI UI

IMPORTANT:
Do NOT assume features exist unless explicitly listed as implemented.

## AI Behavior Rules

- Do NOT overengineer solutions
- Prefer simple, pragmatic implementations
- Avoid unnecessary abstractions
- Always align suggestions with current architecture
- When proposing changes, provide 2–3 options with pros/cons
- Do NOT rewrite large parts of the system unless explicitly requested

## Numerical Methods Roadmap

This section defines planned and partially implemented numerical methods in the system.

### Root Finding (Корені рівнянь) — COMPLETED
All core methods are already implemented in Python + exposed via API + UI.

- Bisection method (Дихотомія)
- Simple iteration method (Метод простих ітерацій)
- Newton-Raphson method (Метод Ньютона)
- Secant method (Метод січних)
- Combined method (Newton + Secant / Brent-like hybrid)

---

### Systems of Equations (Системи рівнянь)

#### Linear Systems — COMPLETED
- Cramer's rule (Крамер) — implemented
- Gaussian elimination (Метод Гаусса) — implemented

#### Non-linear Systems — COMPLETED
- Fixed-point iteration method — implemented
- Seidel method (Gauss-Seidel) — implemented

---

### Interpolation (Інтерполяція) — COMPLETED
- Newton interpolation polynomial — implemented
- Lagrange interpolation polynomial — implemented
- Spline interpolation (cubic splines, SciPy) — implemented

---

### Numerical Differentiation (Чисельне диференціювання) — COMPLETED
- Finite differences: forward, backward, central (1st and 2nd order) — implemented
- Derivative via Lagrange interpolation — implemented

---

### Numerical Integration (Чисельне інтегрування) — COMPLETED
- Rectangle rule (left, right, midpoint) — implemented
- Trapezoidal rule — implemented
- Simpson’s 1/3 rule — implemented

---

### Optimization (Оптимізація функцій) — TODO

#### One-dimensional optimization
- Uniform search (brute grid search)
- Golden section search (Золотий перетин)

#### Multi-dimensional optimization
- Gradient descent method

---

### Ordinary Differential Equations (ОДР) — TODO
- Picard method
- Euler method (simple and improved)
- Runge-Kutta methods (2nd and 4th order)