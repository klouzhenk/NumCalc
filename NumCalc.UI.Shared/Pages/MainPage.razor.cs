namespace NumCalc.UI.Shared.Pages;

public partial class MainPage : BasePage<MainPage>
{
    private record CategoryCard(string Icon, string TitleKey, string DescKey, string Route, string[] Methods);

    private static readonly List<CategoryCard> Categories =
    [
        new("zero-function", "Roots", "RootsDesc", "/root-finding",
            ["Bisection", "Newton-Raphson", "Secant", "Simple Iteration", "Combined (Brent)"]),

        new("equation-system", "EquationSystems", "EquationSystemsDesc", "/equation-systems",
            ["Cramer's Rule", "Gaussian Elimination", "Fixed-point Iteration", "Gauss-Seidel"]),

        new("interpolation", "Interpolation", "InterpolationDesc", "/interpolation",
            ["Newton Polynomial", "Lagrange Polynomial", "Cubic Spline"]),

        new("differentiation", "Differentiation", "DifferentiationDesc", "/differentiation",
            ["Forward / Backward / Central Differences", "Lagrange Derivative"]),

        new("integration", "Integration", "IntegrationDesc", "/integration",
            ["Rectangle Rule", "Trapezoid Rule", "Simpson's 1/3 Rule"]),

        new("optimization", "Optimization", "OptimizationDesc", "/optimization",
            ["Uniform Search", "Golden Section", "Gradient Descent"]),

        new("ode", "Ode", "OdeDesc", "/ode",
            ["Euler", "Euler Improved (Heun)", "Runge-Kutta 2", "Runge-Kutta 4", "Picard"]),
    ];
}
