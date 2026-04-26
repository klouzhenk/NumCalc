using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NumCalc.Shared.Common;
using NumCalc.Shared.Differentiation.Requests;
using NumCalc.Shared.Differentiation.Responses;
using NumCalc.Shared.Enums.Differentiation;
using NumCalc.UI.Shared.Components;
using NumCalc.UI.Shared.Components.Differentiation;
using NumCalc.UI.Shared.Models.Differentiation;
using NumCalc.UI.Shared.Enums.Charts;
using NumCalc.UI.Shared.Enums.Differentiation;
using NumCalc.UI.Shared.Enums.Roots;
using FiniteDiffVariant = NumCalc.Shared.Enums.Differentiation.FiniteDiffVariant;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.Charts;
using NumCalc.UI.Shared.Models.Export;
using NumCalc.UI.Shared.Services.Interfaces;
using System.Text.Json;
using NumCalc.UI.Shared.Models.User;
using NumCalc.UI.Shared.Models.User.Enums;
using NumCalc.UI.Shared.Utils;

namespace NumCalc.UI.Shared.Pages;

public partial class Differentiation : BasePage<Differentiation>
{
    private const string ChartContainerId = "chart--differentiation";

    [Inject] private ICalculationApiService CalculationApiService { get; set; } = null!;
    [Inject] public IPdfExportService PdfExportService { get; set; } = null!;

    private AnalysisMode _mode = AnalysisMode.Single;
    private DifferentiationMethod _method = DifferentiationMethod.FiniteDifferences;
    private FiniteDiffVariant _variant = FiniteDiffVariant.Central;
    private DifferentiationInputMode _inputMode = DifferentiationInputMode.Function;
    private List<DifferentiationComparisonMethod> _benchmarkMethods = [];
    private DifferentiationInput? _input;
    private DifferentiationResponse? Result { get; set; }
    private DifferentiationComparisonResponse? ComparisonResult { get; set; }
    private SavedInputPickerModal? _picker;
    private bool _showSaveForm;
    private string _saveInputName = string.Empty;

    private void ResetResult()
    {
        Result = null;
        ComparisonResult = null;
    }

    private double _queryPoint;
    private string? _lastExpression;
    private double _lastStepSize;
    private int _lastDerivativeOrder;

    private bool IsChartVisible => Result?.ChartData is not null;

    private async Task Calculate()
    {
        Result = null;
        ComparisonResult = null;

        if (_input is null) return;

        var formData = await _input.GetFormData();
        _queryPoint = formData.QueryPoint;
        _lastExpression = formData.FunctionExpression;
        _lastStepSize = formData.StepSize;
        _lastDerivativeOrder = formData.DerivativeOrder;

        if (_mode is AnalysisMode.Benchmark)
        {
            if (_benchmarkMethods.Count == 0)
            {
                UiService.ShowError(Localizer["SelectAtLeastOneMethod"]);
                return;
            }

            var compRequest = new DifferentiationComparisonRequest
            {
                FunctionExpression = formData.FunctionExpression ?? string.Empty,
                XNodes = formData.XNodes,
                QueryPoint = formData.QueryPoint,
                StepSize = formData.StepSize,
                DerivativeOrder = formData.DerivativeOrder,
                Methods = _benchmarkMethods
            };

            ComparisonResult = await SafeExecuteAsync(() => CalculationApiService.GetDifferentiationComparisonAsync(compRequest));
            return;
        }

        var request = new DifferentiationRequest
        {
            Mode = formData.Mode,
            FunctionExpression = formData.FunctionExpression,
            XNodes = formData.XNodes,
            YValues = formData.YValues,
            QueryPoint = formData.QueryPoint,
            StepSize = formData.StepSize,
            DerivativeOrder = formData.DerivativeOrder
        };

        Func<Task<DifferentiationResponse?>> apiCall = _method switch
        {
            DifferentiationMethod.FiniteDifferences => () => CalculationApiService.DifferentiateFiniteDiffAsync(request, _variant),
            DifferentiationMethod.Lagrange           => () => CalculationApiService.DifferentiateLagrangeAsync(request),
            _ => throw new ArgumentOutOfRangeException(nameof(_method))
        };

        Result = await SafeExecuteAsync(apiCall);

        if (Result is not null)
        {
            var methodLabel = _method is DifferentiationMethod.FiniteDifferences
                ? $"Finite Differences ({_variant})"
                : "Lagrange";
            var inputs = new Dictionary<string, string>
            {
                ["Method"] = methodLabel,
                ["Query Point"] = formData.QueryPoint.ToString("G"),
                ["Derivative Order"] = formData.DerivativeOrder.ToString()
            };
            if (!string.IsNullOrWhiteSpace(formData.FunctionExpression))
                inputs["Expression"] = formData.FunctionExpression;
            if (_method is DifferentiationMethod.FiniteDifferences)
                inputs["Step Size"] = formData.StepSize.ToString("G");

            var order = formData.DerivativeOrder == 2 ? "f''" : "f'";
            await TrySaveHistoryAsync(new SaveCalculationRecordRequest
            {
                Type = CalculationType.Differentiation,
                MethodName = methodLabel,
                InputsJson = JsonSerializer.Serialize(inputs),
                ResultSummary = $"{order}(x*) = {Result.DerivativeValue:G10}",
                ExecutionTimeMs = Result.ExecutionTimeMs
            });

            await UpdateChart();
        }
    }

    private async Task UpdateChart()
    {
        if (Result?.ChartData is null) return;

        var chartData = Result.ChartData
            .Where(p => p.X.HasValue && p.Y.HasValue)
            .Select(p => new double[] { p.X!.Value, p.Y!.Value })
            .ToList();

        if (chartData.Count == 0) return;

        var xMin = chartData.Min(p => p[0]);
        var xMax = chartData.Max(p => p[0]);
        var nearest = chartData.MinBy(p => Math.Abs(p[0] - _queryPoint));
        var fAtXStar = nearest![1];

        var series = new List<ChartSeries>
        {
            new()
            {
                Name = "f(x)",
                Data = chartData,
                Color = ColorUtils.GetColor(Enums.Color.Primary),
                LineWidth = 2,
                IsVisible = true
            }
        };

        if (_lastDerivativeOrder == 1)
        {
            series.Add(new ChartSeries
            {
                Name = "Tangent at x*",
                Data =
                [
                    [xMin, fAtXStar + Result!.DerivativeValue * (xMin - _queryPoint)],
                    [xMax, fAtXStar + Result!.DerivativeValue * (xMax - _queryPoint)]
                ],
                Color = ColorUtils.GetColor(Enums.Color.PrimaryDark),
                LineWidth = 1,
                IsVisible = true
            });
        }

        series.Add(new ChartSeries
        {
            Name = "x*",
            Type = ChartType.Scatter,
            Data = [[_queryPoint, fAtXStar]],
            Color = ColorUtils.GetColor(Enums.Color.PrimaryDark),
            IsVisible = true,
            Marker = new ChartMarker { Radius = 8, Symbol = ChartSymbolType.Circle }
        });

        var config = new Chart
        {
            ContainerId = ChartContainerId,
            Title = null,
            XAxis = new ChartAxis { Title = "x", PlotLines = [ChartUtils.CreateZeroLine()] },
            YAxis = new ChartAxis { Title = "f(x)", PlotLines = [ChartUtils.CreateZeroLine()] },
            Series = series
        };

        await JsRuntime.InvokeVoidAsync("NumCalc.drawPlot", config);
    }

    private Task OpenPickerAsync() => _picker?.ShowAsync() ?? Task.CompletedTask;

    private async Task ConfirmSaveAsync()
    {
        if (string.IsNullOrWhiteSpace(_saveInputName) || _input is null) return;
        var data = await _input.GetFormData();
        await TrySaveInputAsync(_saveInputName, CalculationType.Differentiation, JsonSerializer.Serialize(data));
        _saveInputName = string.Empty;
        _showSaveForm = false;
    }

    private async Task LoadFromJsonAsync(string json)
    {
        if (_input is null) return;
        var data = JsonSerializer.Deserialize<DifferentiationFormData>(json);
        if (data is null) return;
        _inputMode = data.Mode;
        StateHasChanged();
        await _input.SetFormDataAsync(data);
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

            var chartImage = IsChartVisible
                ? await JsRuntime.InvokeAsync<string>("PdfHelper.getChartImage", ChartContainerId)
                : null;

            var methodLabel = _method is DifferentiationMethod.FiniteDifferences
                ? $"Finite Differences ({_variant})"
                : "Lagrange";

            var inputs = new Dictionary<string, string>
            {
                ["Method"] = methodLabel,
                ["Query Point"] = _queryPoint.ToString("G"),
                ["Derivative Order"] = _lastDerivativeOrder.ToString()
            };
            if (!string.IsNullOrWhiteSpace(_lastExpression))
                inputs["Expression"] = _lastExpression;
            if (_method is DifferentiationMethod.FiniteDifferences)
                inputs["Step Size"] = _lastStepSize.ToString("G");

            var order = _lastDerivativeOrder == 2 ? "f''" : "f'";
            var resultStr = $"{order}(x*) = {Result.DerivativeValue:G10}";

            var request = new SavedFileRequest
            {
                MethodName = $"Differentiation — {methodLabel}",
                Inputs = inputs,
                Result = resultStr,
                Steps = steps,
                ChartImage = chartImage
            };

            var pdfBytes = PdfExportService.GeneratePdf(request);
            var fileName = $"differentiation-{_method}.pdf";
            await TrySaveFileAsync(fileName, pdfBytes, CalculationType.Differentiation, $"Differentiation — {methodLabel}");
            var base64 = Convert.ToBase64String(pdfBytes);
            await JsRuntime.InvokeVoidAsync("PdfHelper.downloadFile", fileName, "application/pdf", base64);
        });
    }
}
