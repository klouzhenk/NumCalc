using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NumCalc.Shared.EquationsSystems.Requests;
using NumCalc.Shared.EquationsSystems.Responses;
using NumCalc.UI.Shared.Components;
using NumCalc.UI.Shared.Components.EquationSystems;
using NumCalc.UI.Shared.Enums.EquationSystems;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.Export;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.Pages;

public partial class EquationSystems : BasePage<EquationSystems>
{
    [Inject] private ICalculationApiService CalculationApiService { get; set; } = null!;
    [Inject] public IPdfExportService PdfExportService { get; set; } = null!;

    private EquationSystemCategory Category { get; set; } = EquationSystemCategory.Linear;
    private LinearSystemMethod LinearMethod { get; set; } = LinearSystemMethod.Cramer;
    private NonLinearSystemMethod NonLinearMethod { get; set; } = NonLinearSystemMethod.FixedPoint;
    private int Size { get; set; } = 2;

    private readonly int[] _sizes = [2, 3, 4];

    private LinearSystemInput? _linearInput;
    private EquationList? _equationList;

    private SystemSolvingResponse? Result { get; set; }
    private List<string>? _lastEquations;
    private List<string>? _lastVariables;

    private async Task Calculate()
    {
        Result = null;

        if (Category is EquationSystemCategory.Linear)
            await CalculateLinear();
        else
            await CalculateNonLinear();
    }

    private async Task CalculateLinear()
    {
        if (_linearInput is null) return;

        var variables = Enumerable.Range(1, Size).Select(i => $"x{i}").ToList();

        var equations = BuildEquationStrings(_linearInput.Coefficients, _linearInput.Rhs, variables);
        _lastEquations = equations;
        _lastVariables = variables;

        var request = new SystemSolvingRequest
        {
            Equations = equations,
            Variables = variables
        };

        Func<Task<SystemSolvingResponse?>> apiCall = LinearMethod switch
        {
            LinearSystemMethod.Cramer => () => CalculationApiService.SolveCramerAsync(request),
            LinearSystemMethod.Gauss  => () => CalculationApiService.SolveGaussianAsync(request),
            _ => throw new ArgumentOutOfRangeException(nameof(LinearMethod))
        };

        Result = await SafeExecuteAsync(apiCall);
    }

    private async Task CalculateNonLinear()
    {
        if (_equationList is null) return;

        var formData = await _equationList.GetFormData();

        if (formData.IterationFunctions.Any(string.IsNullOrWhiteSpace))
        {
            UiService.ShowError(Localizer["ExpressionRequired"]);
            return;
        }

        _lastEquations = formData.IterationFunctions.ToList();
        _lastVariables = formData.Variables.ToList();

        var request = new NonLinearSystemRequest
        {
            IterationFunctions = formData.IterationFunctions.ToList(),
            Variables = formData.Variables.ToList(),
            InitialGuess = formData.InitialGuess.ToList(),
            Tolerance = formData.Tolerance,
            MaxIterations = formData.MaxIterations
        };

        Func<Task<SystemSolvingResponse?>> apiCall = NonLinearMethod switch
        {
            NonLinearSystemMethod.FixedPoint => () => CalculationApiService.SolveFixedPointAsync(request),
            NonLinearSystemMethod.Seidel     => () => CalculationApiService.SolveSeidelAsync(request),
            _ => throw new ArgumentOutOfRangeException(nameof(NonLinearMethod))
        };

        Result = await SafeExecuteAsync(apiCall);
    }

    private async Task ExportPdfAsync()
    {
        if (Result is null) return;

        var steps = new List<StepExportItem>();
        foreach (var step in Result.SolutionSteps ?? [])
        {
            string? imageBase64 = null;
            if (!string.IsNullOrWhiteSpace(step.LatexFormula))
                imageBase64 = await JsRuntime.InvokeAsync<string>("PdfHelper.renderLatexToPng", step.LatexFormula);
            steps.Add(new StepExportItem { Description = step.Description, ImageBase64 = imageBase64, Value = step.Value });
        }

        var methodName = Category is EquationSystemCategory.Linear
            ? $"Equation Systems — {LinearMethod}"
            : $"Equation Systems — {NonLinearMethod}";

        var inputs = new Dictionary<string, string>
        {
            ["Category"] = Category.ToString(),
            ["Method"] = Category is EquationSystemCategory.Linear ? LinearMethod.ToString() : NonLinearMethod.ToString()
        };
        if (_lastEquations is { Count: > 0 })
        {
            var label = Category is EquationSystemCategory.Linear ? "Equation" : "Iteration Function";
            for (var i = 0; i < _lastEquations.Count; i++)
                inputs[$"{label} {i + 1}"] = _lastEquations[i];
        }
        if (_lastVariables is { Count: > 0 })
            inputs["Variables"] = string.Join(", ", _lastVariables);

        var resultStr = Result.Roots is { Count: > 0 }
            ? string.Join(",  ", Result.Roots.Select((r, i) => $"x{i + 1} = {r}"))
            : "No solution found";

        var request = new PdfExportRequest
        {
            MethodName = methodName,
            Inputs = inputs,
            Result = resultStr,
            Steps = steps
        };

        var pdfBytes = PdfExportService.GeneratePdf(request);
        var base64 = Convert.ToBase64String(pdfBytes);
        await JsRuntime.InvokeVoidAsync("PdfHelper.downloadFile",
            $"equation-systems-{(Category is EquationSystemCategory.Linear ? LinearMethod : NonLinearMethod)}.pdf",
            "application/pdf", base64);
    }

    private static List<string> BuildEquationStrings(double[,] coefficients, double[] rhs, List<string> variables)
    {
        var size = variables.Count;
        var equations = new List<string>(size);

        for (var row = 0; row < size; row++)
        {
            var terms = Enumerable.Range(0, size)
                .Select(col => $"{coefficients[row, col]}*{variables[col]}");
            equations.Add($"{string.Join(" + ", terms)} = {rhs[row]}");
        }

        return equations;
    }
}
