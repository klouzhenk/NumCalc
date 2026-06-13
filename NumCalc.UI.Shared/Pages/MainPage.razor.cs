namespace NumCalc.UI.Shared.Pages;

public partial class MainPage : BasePage<MainPage>
{
    private record CategoryCard(string Icon, string TitleKey, string DescKey, string Route, string[] Methods);

    private static readonly List<CategoryCard> Categories =
    [
        new("zero-function", "RootFinding", "RootFindingDesc", "/root-finding",
            ["Dichotomy", "Newton", "SimpleIterations", "Secant", "Combined"]),

        new("equation-system", "EquationSystems", "EquationSystemsDesc", "/equation-systems",
            ["Cramer", "Gauss", "FixedPoint", "Seidel"]),

        new("interpolation", "Interpolation", "InterpolationDesc", "/interpolation",
            ["Newton", "Lagrange", "Spline"]),

        new("differentiation", "Differentiation", "DifferentiationDesc", "/differentiation",
            ["Forward", "Backward", "Central", "Lagrange"]),

        new("integration", "Integration", "IntegrationDesc", "/integration",
            ["Rectangle", "Trapezoid", "Simpson"]),

        new("optimization", "Optimization", "OptimizationDesc", "/optimization",
            ["UniformSearch", "GoldenSection", "GradientDescent"]),

        new("ode", "Ode", "OdeDesc", "/ode",
            ["Euler", "EulerImproved", "RungeKutta2", "RungeKutta4", "Picard"]),
    ];
}
