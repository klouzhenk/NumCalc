using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NumCalc.Shared.EquationsSystems.Requests;
using NumCalc.Shared.EquationsSystems.Responses;
using NumCalc.UI.Shared.Components;
using NumCalc.UI.Shared.Components.EquationSystems;
using NumCalc.UI.Shared.Enums.EquationSystems;
using NumCalc.UI.Shared.Enums.Roots;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Enums.Charts;
using NumCalc.UI.Shared.Models.Charts;
using NumCalc.UI.Shared.Models.Export;
using NumCalc.UI.Shared.Services.Interfaces;
using NumCalc.UI.Shared.Utils;

namespace NumCalc.UI.Shared.Pages;

public partial class EquationSystems : BasePage<EquationSystems>
{
    [Inject] private ICalculationApiService CalculationApiService { get; set; } = null!;
    [Inject] public IPdfExportService PdfExportService { get; set; } = null!;

    private AnalysisMode _mode = AnalysisMode.Single;
    private EquationSystemCategory Category { get; set; } = EquationSystemCategory.Linear;
    private LinearSystemMethod LinearMethod { get; set; } = LinearSystemMethod.Cramer;
    private NonLinearSystemMethod NonLinearMethod { get; set; } = NonLinearSystemMethod.FixedPoint;
    private int Size { get; set; } = 2;

    private const string ChartContainerId = "chart--equation-systems";

    private readonly int[] _sizes = [2, 3, 4];

    private LinearSystemInput? _linearInput;
    private EquationList? _equationList;

    private SystemSolvingResponse? Result { get; set; }
    private LinearSystemComparisonResponse? LinearComparisonResult { get; set; }
    private NonLinearSystemComparisonResponse? NonLinearComparisonResult { get; set; }
    private List<string>? _lastEquations;
    private List<string>? _lastVariables;

    private bool IsChartVisible => Result?.ChartSeries is { Count: > 0 };

    private void ResetResult()
    {
        Result = null;
        LinearComparisonResult = null;
        NonLinearComparisonResult = null;
    }

    private async Task Calculate()
    {
        Result = null;
        LinearComparisonResult = null;
        NonLinearComparisonResult = null;

        if (_mode is AnalysisMode.Benchmark)
        {
            if (Category is EquationSystemCategory.Linear)
                await CompareLinear();
            else
                await CompareNonLinear();
            return;
        }

        if (Category is EquationSystemCategory.Linear)
            await CalculateLinear();
        else
            await CalculateNonLinear();
    }

    private async Task CompareLinear()
    {
        if (_linearInput is null) return;

        var variables = Enumerable.Range(1, Size).Select(i => $"x{i}").ToList();
        var equations = BuildEquationStrings(_linearInput.Coefficients, _linearInput.Rhs, variables);

        var request = new LinearSystemComparisonRequest
        {
            Equations = equations,
            Variables = variables
        };

        LinearComparisonResult = await SafeExecuteAsync(() => CalculationApiService.GetLinearComparisonAsync(request));
    }

    private async Task CompareNonLinear()
    {
        if (_equationList is null) return;

        var formData = await _equationList.GetFormData();

        if (formData.IterationFunctions.Any(string.IsNullOrWhiteSpace))
        {
            UiService.ShowError(Localizer["ExpressionRequired"]);
            return;
        }

        var request = new NonLinearSystemComparisonRequest
        {
            IterationFunctions = formData.IterationFunctions.ToList(),
            Variables = formData.Variables.ToList(),
            InitialGuess = formData.InitialGuess.ToList(),
            Tolerance = formData.Tolerance,
            MaxIterations = formData.MaxIterations
        };

        NonLinearComparisonResult = await SafeExecuteAsync(() => CalculationApiService.GetNonLinearComparisonAsync(request));
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

        if (Result is not null)
            await UpdateChart();
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

        if (Result is not null)
            await UpdateChart();
    }

    private async Task UpdateChart()
    {
        if (Result?.ChartSeries is not { Count: > 0 }) return;

        var x1Name = _lastVariables?.ElementAtOrDefault(0) ?? "x\u2081";
        var x2Name = _lastVariables?.ElementAtOrDefault(1) ?? "x\u2082";

        var is3D = Result.ChartSeries.Any(s => s.Points.Any(p => p.Z.HasValue));

        if (is3D)
        {
            var x3Name = _lastVariables?.ElementAtOrDefault(2) ?? "x\u2083";

            var series3d = Result.ChartSeries
                .Select((s, idx) => new ChartSeries
                {
                    Name = s.Label,
                    Data = s.Points
                        .Where(p => p.X.HasValue && p.Y.HasValue && p.Z.HasValue)
                        .Select(p => new double[] { p.X!.Value, p.Y!.Value, p.Z!.Value })
                        .ToList(),
                    Color = ColorUtils.GetSeriesColor(idx),
                    IsVisible = true
                })
                .ToList();

            if (Result.Roots is { Count: >= 3 })
            {
                series3d.Add(new ChartSeries
                {
                    Name = "Solution",
                    Data = [[Result.Roots[0], Result.Roots[1], Result.Roots[2]]],
                    Type = Enums.Charts.ChartType.Scatter,
                    Color = ColorUtils.GetColor(Enums.Color.Danger),
                    IsVisible = true,
                    Marker = new ChartMarker { Radius = 8, Symbol = ChartSymbolType.Circle }
                });
            }

            var config3d = new Chart
            {
                ContainerId = ChartContainerId,
                ShowLegend = true,
                XAxis = new ChartAxis { Title = x1Name },
                YAxis = new ChartAxis { Title = x2Name },
                ZAxis = new ChartAxis { Title = x3Name }
            };
            config3d.Series.AddRange(series3d);

            await JsRuntime.InvokeVoidAsync("NumCalc.drawPlot3d", config3d);
        }
        else
        {
            var series = Result.ChartSeries
                .Select((s, idx) => new ChartSeries
                {
                    Name = s.Label,
                    Data = s.Points
                        .Where(p => p.X.HasValue && p.Y.HasValue)
                        .Select(p => new double[] { p.X!.Value, p.Y!.Value })
                        .ToList(),
                    Color = ColorUtils.GetSeriesColor(idx),
                    LineWidth = 2,
                    IsVisible = true
                })
                .ToList();

            if (Result.Roots is { Count: >= 2 })
            {
                series.Add(new ChartSeries
                {
                    Name = "Solution",
                    Data = [[Result.Roots[0], Result.Roots[1]]],
                    Type = Enums.Charts.ChartType.Scatter,
                    Color = ColorUtils.GetColor(Enums.Color.Danger),
                    LineWidth = 0,
                    ZIndex = 5,
                    IsVisible = true,
                    Marker = new ChartMarker { Radius = 8, Symbol = ChartSymbolType.Circle }
                });
            }

            var config = new Chart
            {
                ContainerId = ChartContainerId,
                ShowLegend = true,
                XAxis = new ChartAxis { Title = x1Name, PlotLines = [ChartUtils.CreateZeroLine()] },
                YAxis = new ChartAxis { Title = x2Name, PlotLines = [ChartUtils.CreateZeroLine()] }
            };
            config.Series.AddRange(series);

            await JsRuntime.InvokeVoidAsync("NumCalc.drawPlot", config);
        }
    }

    private async Task ExportPdfAsync()
    {
        if (Result is null) return;
        await SafeExecuteAsync(async () =>
        {
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

            var chartImage = IsChartVisible
                ? await JsRuntime.InvokeAsync<string>("PdfHelper.getChartImage", ChartContainerId)
                : null;

            var request = new PdfExportRequest
            {
                MethodName = methodName,
                Inputs = inputs,
                Result = resultStr,
                Steps = steps,
                ChartImage = chartImage
            };

            var pdfBytes = PdfExportService.GeneratePdf(request);
            var base64 = Convert.ToBase64String(pdfBytes);
            await JsRuntime.InvokeVoidAsync("PdfHelper.downloadFile",
                $"equation-systems-{(Category is EquationSystemCategory.Linear ? LinearMethod : NonLinearMethod)}.pdf",
                "application/pdf", base64);
        });
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
