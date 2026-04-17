using Microsoft.AspNetCore.Components;

namespace NumCalc.UI.Shared.Pages;

public partial class MainPage : BasePage<MainPage>
{
    private record CategoryCard(string Icon, string TitleKey, string Route, string[] Methods);

    private static readonly List<CategoryCard> Categories =
    [
        new("zero-function", "Roots", "/root-finding",
            ["Bisection", "Newton-Raphson", "Secant", "Simple Iteration", "Combined (Brent)"]),

        new("equation-system", "EquationSystems", "/equation-systems",
            ["Cramer's Rule", "Gaussian Elimination", "Fixed-point Iteration", "Gauss-Seidel"]),

        new("interpolation", "Interpolation", "/interpolation",
            ["Newton Polynomial", "Lagrange Polynomial", "Cubic Spline"]),

        new("differentiation", "Differentiation", "/differentiation",
            ["Forward / Backward / Central Differences", "Lagrange Derivative"]),

        new("integration", "Integration", "/integration",
            ["Rectangle Rule", "Trapezoid Rule", "Simpson's 1/3 Rule"]),

        new("optimization", "Optimization", "/optimization",
            ["Uniform Search", "Golden Section", "Gradient Descent"]),

        new("ode", "Ode", "/ode",
            ["Euler", "Euler Improved (Heun)", "Runge-Kutta 2", "Runge-Kutta 4", "Picard"]),
    ];
}
