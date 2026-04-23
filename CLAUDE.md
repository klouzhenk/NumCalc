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

- **Controllers:** `RootFindingController` (5 methods + comparison endpoint), `EquationsSystemsController` (Cramer, Gaussian, Fixed-point, Seidel), `InterpolationController` (Newton, Lagrange, Spline), `DifferentiationController` (finite-diff, Lagrange derivative), `IntegrationController` (rectangle, trapezoid, Simpson), `OptimizationController` (uniform-search, golden-section, gradient-descent), `OdeController` (Euler, Euler Improved, RK2, RK4, Picard)
- **Services:** `IRootFindingService` / `IEquationsSystemService` / `IInterpolationService` / `IDifferentiationService` / `IIntegrationService` / `IOptimizationService` / `IOdeService` — call into Python via CSnakes
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
  optimization/
    uniform_search.py — Brute grid search over [a, b]
    golden_section.py — Golden section interval search
    gradient_descent.py — Gradient descent (N-D, auto-detects variables from expression)
  odes/
    euler.py          — Euler method
    euler_improved.py — Euler improved (Heun) method
    runge_kutta_2.py  — Runge-Kutta 2nd order
    runge_kutta_4.py  — Runge-Kutta 4th order
    picard.py         — Picard successive approximation (symbolic, SymPy)
  shared/
    functions.py      — Function parsing/evaluation
    parsing.py
    structures.py     — Shared data structures
```

Top-level dispatcher scripts (CSnakes entry points): `root_finding.py`, `equation_systems.py`, `interpolation.py`, `differentiation.py`, `integration.py`, `optimization.py`, `ode.py`, `warming_up.py`.

**CSnakes naming rule:** Sub-module filenames must be unique across the entire `Scripts/` tree (CSnakes generates proxy hint names from filename only, ignoring folder path). A duplicate filename anywhere causes the entire generator to fail. Also, a top-level `foo.py` and a `foo/` subfolder must not coexist — the `foo/__init__.py` collides with `foo.py`. Sub-folders do not need `__init__.py` (Python 3 namespace packages).

### Shared DTOs (`NumCalc.Shared`)

- Request/response classes for root finding, equation systems, interpolation, differentiation, integration, optimization, and ODE
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
- `OptimizationRequest` — `FunctionExpression`, `LowerBound`, `UpperBound`, `Points` (default 100), `Tolerance` (default 1e-6)
- `GradientDescentRequest` — `FunctionExpression`, `InitialPoint[]`, `LearningRate` (0.01), `Tolerance` (1e-6), `MaxIterations` (200)
- `OptimizationResponse` — `MinimumValue`, `ArgMinX?` (2D methods), `ArgMinPoint?` (gradient descent), `ChartData?`, `SolutionSteps`, `ExecutionTimeMs`
- `OdeRequest` — `FunctionExpression`, `InitialX`, `InitialY`, `TargetX`, `StepSize` (default 0.1, must be positive), `PicardOrder` (1–10, default 4)
- `OdeResponse` — `SolutionPoints` (List<Point>), `PolynomialLatex?` (Picard only), `SolutionSteps`, `ExecutionTimeMs`

### UI Shared Library (`NumCalc.UI.Shared`)

- **HTTP layer:** `BaseApiService` (abstract base) → `ICalculationApiService` / `CalculationApiService` (single concrete client covering all 7 topic areas)
- **All page components** live in `NumCalc.UI.Shared/Pages/` — `RootFinding.razor`, `EquationSystems.razor`, `Interpolation.razor`, `Differentiation.razor`, `Integration.razor`, `Optimization.razor`, `Ode.razor`, `MainPage.razor`
- **Reusable components** in `Components/`: `MathInput`, `Tooltip`, `TopicInfo`, `SolutionStepsList`, `NodeTable`, `LinearSystemInput`, `Dropdown`, `Switch`, `HamburgerMenu`, `Header`, `BaseModal`, `OcrInput`, per-domain input components (`OdeInput`, `OptimizationInput`, `InterpolationInput`, `DifferentiationInput`, `IntegrationInput`, `EquationList`)
- **TopicInfo components** in `Components/TopicInfos/`: one Razor component per topic (`RootFindingTopicInfo`, `EquationSystemsTopicInfo`, `InterpolationTopicInfo`, `DifferentiationTopicInfo`, `IntegrationTopicInfo`, `OptimizationTopicInfo`, `OdeTopicInfo`) — rendered from `Header.razor` via a switch on `NavigationItem`
- **Services:** `IPdfExportService` / `PdfExportService`, `IOcrService` / `OcrService`, `IUiStateService` / `UiStateService`, `ICultureService`
- **Layout:** `MainLayout.razor` in `Layouts/`
- Localization resources: `Localization.resx` (English) + `Localization.uk.resx` (Ukrainian)
- Frontend stack: **Highcharts** (charts, via `wwwroot/js/highcharts.js` + `charts.js`), **MathLive** (math input field), **mathjs** (client-side expression evaluation), **Vite** (JS/CSS bundler)
- LaTeX in `SolutionStep.LatexFormula` is rendered via **KaTeX** (html2canvas captures it as PNG for PDF export)

### Web UI (`NumCalc.UI.Web`)

- Server-side Blazor with interactive server rendering
- Contains only `App.razor` and `Routes.razor` — all page and component logic lives in `NumCalc.UI.Shared`
- Localization: supports `en` and `uk` cultures via `RequestLocalizationOptions`
- Communicates with the Calculation API via `ICalculationApiService`

## Key Patterns

- **Python ↔ C# boundary:** Python scripts return JSON strings → C# deserializes into strongly-typed DTOs via `ResponseEnvelope<T>`. Add new numerical methods by adding a Python script and a corresponding service method + controller endpoint.
- **Error handling:** All exceptions surface through `GlobalExceptionHandler`; numerical errors use typed `ErrorCodes` rather than raw exceptions.
- **Localization:** All user-visible strings go in `NumCalc.UI.Shared/Localization/Localization.resx` (and `.uk.resx`). Do not hard-code display text in components.
- **Shared UI components:** New reusable Blazor components belong in `NumCalc.UI.Shared`, not in `NumCalc.UI.Web`, so they can be reused by MAUI.
- **Logging:** Serilog is configured at the host level (`builder.Host.UseSerilog()`) in `NumCalc.Calculation.Api/Program.cs`. All 7 API services inject `ILogger<T>` via primary constructor and log method entry + completion with key parameters. `GlobalExceptionHandler` logs warnings/errors. UI pages log via `BasePage<T>.Logger`. `PythonWarmingUpService` logs warmup start/completion.

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
- Optimization — fully implemented end-to-end (Python + API + HTTP client + Blazor UI):
  - Uniform search (brute grid search over [a, b])
  - Golden section search
  - Gradient descent (N-D; auto-detects variables alphabetically from expression; chart for single-variable case)
- Ordinary Differential Equations (ODE) — fully implemented end-to-end (Python + API + HTTP client + Blazor UI):
  - Euler method
  - Euler Improved (Heun) method
  - Runge-Kutta 2nd order
  - Runge-Kutta 4th order
  - Picard successive approximation (symbolic, SymPy; returns `PolynomialLatex`; chart shows vertical x₀ plot line)
  - Single `OdeInput` component; Picard warning callout + `PicardOrder` select (1–10) shown only when Picard selected
  - Step size `h` in `<details>` advanced section with live computed step count
- Python integration via CSnakes
- Blazor UI for root finding, equation systems, interpolation, differentiation, integration, optimization, and ODE
- PDF export — fully implemented on all 7 result pages (QuestPDF + KaTeX + html2canvas)

Not implemented yet:
- Comparison feature for Interpolation, Integration, ODE, Differentiation, Optimization, Equation Systems pages (root finding comparison already done)
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
- **Comparison** — COMPLETED (`POST api/rootfinding/comparison`; `RootFindingComparisonRequest` → `RootFindingComparisonResponse` with `List<BenchmarkResultDto>` + `BestMethod`)

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
- **Comparison** — COMPLETED (`POST api/interpolation/comparison`; `InterpolationComparisonRequest` → `InterpolationComparisonResponse`)

---

### Numerical Differentiation (Чисельне диференціювання) — COMPLETED
- Finite differences: forward, backward, central (1st and 2nd order) — implemented
- Derivative via Lagrange interpolation — implemented
- **Comparison** — COMPLETED (`POST api/differentiation/finite-diff?variant=X` + `POST api/differentiation/comparison`; `DifferentiationComparisonRequest` → `DifferentiationComparisonResponse`)

---

### Numerical Integration (Чисельне інтегрування) — COMPLETED
- Rectangle rule (left, right, midpoint) — implemented
- Trapezoidal rule — implemented
- Simpson’s 1/3 rule — implemented
- **Comparison** — COMPLETED (`POST api/integration/comparison`; `IntegrationComparisonRequest` → `IntegrationComparisonResponse`)

---

### Optimization (Оптимізація функцій) — COMPLETED

#### One-dimensional optimization
- Uniform search (brute grid search) — implemented
- Golden section search (Золотий перетин) — implemented
- **Comparison** — PLANNED (compare Uniform search + Golden section; gradient descent excluded — different input shape; shared result fields: `MinimumValue`, `ArgMinX`, `ExecutionTimeMs`)

#### Multi-dimensional optimization
- Gradient descent method — implemented

---

### Ordinary Differential Equations (ОДР) — COMPLETED
- Picard method — implemented (symbolic, SymPy)
- Euler method (simple) — implemented
- Euler Improved (Heun) — implemented
- Runge-Kutta 2nd order — implemented
- Runge-Kutta 4th order — implemented
- **Comparison** — COMPLETED (`POST api/ode/comparison`; `OdeComparisonRequest` → `OdeComparisonResponse`; Picard excluded — symbolic/different output)

---

## Method Comparison Feature

Allows users to run multiple methods on the same input and compare results side-by-side. Modeled after the existing root finding comparison.

### Pattern (from root finding reference implementation)

**DTOs (NumCalc.Shared):**
- Request: inherits from the domain's base request + adds `Methods` (e.g. `IEnumerable<InterpolationMethod>`)
- Response: `Results` (`List<ComparisonResultDto>`) + `BestMethod` (selected by `ExecutionTimeMs`, then iteration count as tiebreaker)
- `ComparisonResultDto` per domain: method enum + the scalar result (root / interpolated value / integral / etc.) + `Iterations` (0 for non-iterative methods) + `ExecutionTimeMs`

**API (NumCalc.Calculation.Api):**
- New `POST api/{domain}/comparison` endpoint in the existing controller
- Service method iterates selected methods, calls existing Python solvers individually via `Stopwatch`, collects results
- No new Python code needed — reuses existing per-method functions

**HTTP client (NumCalc.UI.Shared):**
- New method on `ICalculationApiService` / `CalculationApiService`, e.g. `GetInterpolationComparisonAsync()`

**UI (NumCalc.UI.Shared/Pages/):**
- Mode toggle ("Single" / "Compare") on the existing page
- Multi-select for methods (checkboxes or multi-select dropdown)
- Results table: one row per method, columns = method name + result value + iterations + time (ms)
- Best method row highlighted with CSS class `marked`
- No comparison chart needed for most domains (root finding overlays roots on chart — this is domain-specific)

### Domain coverage plan
| Domain | Methods compared | Status |
|---|---|---|
| Root Finding | Bisection, Simple Iter, Newton, Secant, Combined | COMPLETED |
| Integration | Rectangle (L/R/M), Trapezoid, Simpson | COMPLETED |
| Interpolation | Newton, Lagrange, Spline | COMPLETED |
| ODE | Euler, Euler Improved, RK2, RK4 (Picard excluded) | COMPLETED |
| Differentiation | Forward, Backward, Central, Lagrange | COMPLETED |
| Optimization (1D) | Uniform search, Golden section (gradient descent excluded) | PLANNED |
| Equation Systems | Linear: Cramer vs Gaussian / Non-linear: Fixed-point vs Seidel | PLANNED |

### Method enums to add (NumCalc.Shared/Enums/)
- `InterpolationMethod` — Newton, Lagrange, Spline
- `IntegrationMethod` — RectangleLeft, RectangleRight, RectangleMid, Trapezoid, Simpson
- `OdeMethod` — Euler, EulerImproved, RungeKutta2, RungeKutta4

---

## PDF Export — COMPLETED

Target audience: students and teachers who need to include calculation results in lab reports or course materials.

### What the PDF contains
1. Method name and timestamp
2. User inputs (expression, bounds, tolerance, etc.)
3. Solution steps — each `SolutionStep.LatexFormula` rendered as a PNG image via KaTeX + html2canvas
4. Final result value
5. Chart image — captured via html2canvas

### Architecture
- PDF generation: **QuestPDF** (NuGet) — in `NumCalc.UI.Shared`; no changes to `NumCalc.Calculation.Api`
- LaTeX rendering: **KaTeX** (npm) renders each `LatexFormula` into a hidden DOM element; **html2canvas** captures it as a base64 PNG
- Chart export: html2canvas captures the chart container as a PNG embedded in the PDF
- Service: `IPdfExportService` / `PdfExportService` in `NumCalc.UI.Shared/Services/` — takes `PdfExportRequest` (inputs, steps with base64 images, chart image, result), returns `byte[]`
- JS helpers: `pdf-helper.js` — `renderLatexToPng()`, `getChartImage()`, `downloadFile()`
- Trigger: "Export PDF" button on every result page (all 7 pages) calls JS interop to collect images, then `PdfExportService.GeneratePdf()`, then triggers browser download

### Key constraint
QuestPDF cannot render LaTeX directly. All LaTeX is pre-rendered to PNG in the browser before PDF generation.

---

## Help & Tooltip System — COMPLETED

Two distinct components with different purposes and behaviors.

### Component 1 — `<Tooltip>` (input-level hints)

Wraps any label/input and shows a small floating hint when the user hovers the wrapper.

**Usage:**
```razor
<Tooltip>
    <Content>
        <div class="input-group">
            <label>Tolerance</label>
            <input ... />
        </div>
    </Content>
    <Hint>Maximum allowed error ε.</Hint>
</Tooltip>
```

- Renders a `(?)` icon beside the wrapped content
- On hover: small floating text box appears (CSS-driven, no JS required)
- Hint text is passed directly as a `<Hint>` render fragment (no JSON file)
- Lives in `NumCalc.UI.Shared/Components/Tooltip.razor`

---

### Component 2 — `<TopicInfo>` (page-level modal)

A generic modal wrapper. Each topic has its own dedicated Razor component in `Components/TopicInfos/` that wraps `<TopicInfo>` with hard-coded content.

**`TopicInfo.razor` usage (inside a topic component):**
```razor
<TopicInfo Title="..." Subtitle="...">
    <!-- rich content with KaTeX-rendered formulas -->
</TopicInfo>
```

**Rendering:** `Header.razor` switches on `NavigationItem` to render the correct per-topic component (`RootFindingTopicInfo`, `EquationSystemsTopicInfo`, etc.)

- Lives in `NumCalc.UI.Shared/Components/TopicInfo.razor`
- Per-topic components in `NumCalc.UI.Shared/Components/TopicInfos/`
- No JSON files or `HelpContentService` — content is embedded directly in Razor components
- KaTeX (already installed for PDF export) renders LaTeX formulas inside the modals