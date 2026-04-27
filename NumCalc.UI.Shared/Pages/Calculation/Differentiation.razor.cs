using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NumCalc.Shared.Differentiation.Requests;
using NumCalc.Shared.Differentiation.Responses;
using NumCalc.Shared.Enums.Differentiation;
using NumCalc.UI.Shared.Components.Differentiation;
using NumCalc.UI.Shared.Enums.Charts;
using NumCalc.UI.Shared.Enums.Differentiation;
using NumCalc.UI.Shared.Enums.Roots;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.Charts;
using NumCalc.UI.Shared.Models.Differentiation;
using NumCalc.UI.Shared.Models.User;
using NumCalc.UI.Shared.Models.User.Enums;
using NumCalc.UI.Shared.Utils;
using FiniteDiffVariant = NumCalc.Shared.Enums.Differentiation.FiniteDiffVariant;

namespace NumCalc.UI.Shared.Pages.Calculation;

public partial class Differentiation : CalculationPage<Differentiation>
{
    private const string ChartContainerId = "chart--differentiation";

    [Inject] private ICalculationApiService CalculationApiService { get; set; } = null!;

    private AnalysisMode _mode = AnalysisMode.Single;
    private DifferentiationMethod _method = DifferentiationMethod.FiniteDifferences;
    private FiniteDiffVariant _variant = FiniteDiffVariant.Central;
    private DifferentiationInputMode _inputMode = DifferentiationInputMode.Function;
    private List<DifferentiationComparisonMethod> _benchmarkMethods = [];
    private DifferentiationInput? _input;
    private DifferentiationResponse? Result { get; set; }
    private DifferentiationComparisonResponse? ComparisonResult { get; set; }

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

    private async Task SaveInputAsync(string name)
    {
        if (_input is null) return;
        var data = await _input.GetFormData();
        await TrySaveInputAsync(name, CalculationType.Differentiation, JsonSerializer.Serialize(data));
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

        await ExportPdfCoreAsync(
            methodName: $"Differentiation — {methodLabel}",
            inputs: inputs,
            result: resultStr,
            steps: Result.SolutionSteps,
            chartContainerId: IsChartVisible ? ChartContainerId : null,
            fileName: $"differentiation-{_method}.pdf",
            type: CalculationType.Differentiation);
    }
}
