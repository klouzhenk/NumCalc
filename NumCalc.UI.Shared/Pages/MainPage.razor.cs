using Microsoft.AspNetCore.Components;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.User;

namespace NumCalc.UI.Shared.Pages;

public partial class MainPage : BasePage<MainPage>
{
    [Inject] private ICalculationHistoryApiService HistoryApiService { get; set; } = null!;
    [Inject] private ISavedInputApiService SavedInputApiService { get; set; } = null!;
    [Inject] private ISavedFileApiService SavedFileApiService { get; set; } = null!;

    private List<CalculationHistoryDto>? _lastHistory;
    private List<SavedInputDto>? _lastInputs;
    private List<SavedFileMetadataDto>? _lastFiles;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender && IsAuthenticated)
            await LoadUserDataAsync();
    }

    protected override async void OnAuthStateChanged()
    {
        if (IsAuthenticated)
            await LoadUserDataAsync();
        else
        {
            _lastHistory = null;
            _lastInputs = null;
            _lastFiles = null;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task LoadUserDataAsync()
    {
        var historyTask = HistoryApiService.GetLastAsync(5);
        var inputsTask = SavedInputApiService.GetLastAsync(5);
        var filesTask = SavedFileApiService.GetLastAsync(5);

        await Task.WhenAll(historyTask, inputsTask, filesTask);

        _lastHistory = historyTask.Result;
        _lastInputs = inputsTask.Result;
        _lastFiles = filesTask.Result;

        await InvokeAsync(StateHasChanged);
    }

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
