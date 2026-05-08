using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NumCalc.Shared.Common;
using NumCalc.Shared.Enums.Integration;
using NumCalc.Shared.Integration.Requests;
using NumCalc.Shared.Integration.Responses;
using NumCalc.UI.Shared.Components.Integration;
using NumCalc.UI.Shared.Enums.Integration;
using NumCalc.UI.Shared.Enums.Roots;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.Charts;
using NumCalc.UI.Shared.Models.Integration;
using NumCalc.UI.Shared.Models.User;
using NumCalc.UI.Shared.Models.User.Enums;
using NumCalc.UI.Shared.Utils;

namespace NumCalc.UI.Shared.Pages.Calculation;

public partial class Integration : CalculationPage<Integration>
{
    private const string ChartContainerId = "chart--integration";

    [Inject] private ICalculationApiService CalculationApiService { get; set; } = null!;

    private AnalysisMode _mode = AnalysisMode.Single;
    private IntegrationMethod _method = IntegrationMethod.Rectangle;
    private RectangleVariant _rectangleVariant = RectangleVariant.Midpoint;
    private List<IntegrationComparisonMethod> _benchmarkMethods = [];
    private IntegrationInput? _input;
    private IntegrationResponse? Result { get; set; }
    private IntegrationComparisonResponse? ComparisonResult { get; set; }

    private void ResetResult()
    {
        Result = null;
        ComparisonResult = null;
    }

    private bool IsChartVisible => Result?.ChartData is not null;

    private SolutionStep? SelectedStep => Result?.SolutionSteps?.FirstOrDefault();

    private IList<SolutionStep>? FilteredSteps => Result?.SolutionSteps;

    private async Task Calculate()
    {
        Result = null;
        ComparisonResult = null;

        if (_input is null) return;

        var formData = await _input.GetFormData();

        if (_mode is AnalysisMode.Benchmark)
        {
            if (_benchmarkMethods.Count == 0)
            {
                UiService.ShowError(Localizer["SelectAtLeastOneMethod"]);
                return;
            }

            var compRequest = new IntegrationComparisonRequest
            {
                FunctionExpression = formData.FunctionExpression ?? string.Empty,
                LowerBound = formData.LowerBound,
                UpperBound = formData.UpperBound,
                Intervals = formData.Intervals,
                Methods = _benchmarkMethods
            };

            ComparisonResult = await SafeExecuteAsync(() => CalculationApiService.GetIntegrationComparisonAsync(compRequest));
            return;
        }

        var request = new IntegrationRequest
        {
            Mode = IntegrationInputMode.Function,
            FunctionExpression = formData.FunctionExpression,
            LowerBound = formData.LowerBound,
            UpperBound = formData.UpperBound,
            Intervals = formData.Intervals,
            RectangleVariant = _method is IntegrationMethod.Rectangle ? _rectangleVariant : null
        };

        Func<Task<IntegrationResponse?>> apiCall = _method switch
        {
            IntegrationMethod.Rectangle => () => CalculationApiService.IntegrateRectangleAsync(request),
            IntegrationMethod.Trapezoid => () => CalculationApiService.IntegrateTrapezoidAsync(request),
            IntegrationMethod.Simpson   => () => CalculationApiService.IntegrateSimpsonAsync(request),
            _ => throw new ArgumentOutOfRangeException(nameof(_method))
        };

        Result = await SafeExecuteAsync(apiCall);

        if (Result is not null)
        {
            var inputs = new Dictionary<string, string>
            {
                ["Method"] = _method.ToString(),
                ["Expression"] = formData.FunctionExpression ?? string.Empty,
                ["Lower Bound"] = formData.LowerBound.ToString("G"),
                ["Upper Bound"] = formData.UpperBound.ToString("G"),
                ["Intervals"] = formData.Intervals.ToString()
            };
            if (_method is IntegrationMethod.Rectangle)
                inputs["Variant"] = _rectangleVariant.ToString();

            await TrySaveHistoryAsync(new SaveCalculationRecordRequest
            {
                Type = CalculationType.Integration,
                MethodName = _method.ToString(),
                InputsJson = JsonSerializer.Serialize(inputs),
                ResultSummary = $"I = {Result.IntegralValue:G10}",
                ExecutionTimeMs = Result.ExecutionTimeMs
            });

            await UpdateChart(formData);
        }
    }

    private async Task UpdateChart(IntegrationFormData data)
    {
        if (Result?.ChartData is null) return;

        var chartData = Result.ChartData
            .Where(p => p is { X: not null, Y: not null })
            .Select(p => new[] { p.X!.Value, p.Y!.Value })
            .ToList();

        if (chartData.Count == 0) return;

        var useShapes = Result.ShapePoints is not null;

        var curveSeries = new ChartSeries
        {
            Name = "f(x)",
            Data = chartData,
            Color = ColorUtils.GetColor(Enums.Color.Primary),
            LineWidth = 2,
            IsVisible = true,
            FillLowerBound = useShapes ? null : data.LowerBound,
            FillUpperBound = useShapes ? null : data.UpperBound
        };

        var seriesList = new List<ChartSeries>();

        if (useShapes)
        {
            var shapeData = Result.ShapePoints!
                .Where(p => p is { X: not null, Y: not null })
                .Select(p => new[] { p.X!.Value, p.Y!.Value })
                .ToList();

            var shapeName = _method is IntegrationMethod.Rectangle
                ? $"{_rectangleVariant} rectangles"
                : "Trapezoids";

            seriesList.Add(new ChartSeries
            {
                Name = shapeName,
                Data = shapeData,
                Color = ColorUtils.GetColor(Enums.Color.Primary),
                LineWidth = 1,
                IsVisible = true,
                FillLowerBound = data.LowerBound,
                FillUpperBound = data.UpperBound,
                Step = _method is IntegrationMethod.Rectangle ? "left" : null
            });
        }

        seriesList.Add(curveSeries);

        var config = new Chart
        {
            ContainerId = ChartContainerId,
            Title = null,
            XAxis = new ChartAxis
            {
                Title = "x",
                PlotLines = 
                [
                    ChartUtils.CreateZeroLine(),
                    ChartUtils.CreateConstant(data.LowerBound),
                    ChartUtils.CreateConstant(data.UpperBound)
                ]
            },
            YAxis = new ChartAxis { Title = "f(x)", PlotLines = [ChartUtils.CreateZeroLine()] },
            Series = seriesList
        };

        await JsRuntime.InvokeVoidAsync("NumCalc.drawPlot", config);
    }

    private async Task SaveInputAsync(string name)
    {
        if (_input is null) return;
        var data = await _input.GetFormData();
        await TrySaveInputAsync(name, CalculationType.Integration, JsonSerializer.Serialize(data));
    }

    private async Task LoadFromJsonAsync(string json)
    {
        if (_input is null) return;
        var data = JsonSerializer.Deserialize<IntegrationFormData>(json);
        if (data is null) return;
        await _input.SetFormDataAsync(data);
    }

    private async Task ExportPdfAsync()
    {
        if (Result is null) return;
        if (_input is null) return;

        var formData = await _input.GetFormData();

        var inputs = new Dictionary<string, string>
        {
            ["Method"] = _method.ToString(),
            ["Lower Bound"] = formData.LowerBound.ToString("G"),
            ["Upper Bound"] = formData.UpperBound.ToString("G"),
            ["Intervals"] = formData.Intervals.ToString()
        };
        if (!string.IsNullOrWhiteSpace(formData.FunctionExpression))
            inputs["Expression"] = formData.FunctionExpression;
        if (_method is IntegrationMethod.Rectangle)
            inputs["Variant"] = _rectangleVariant.ToString();

        var resultStr = _method is IntegrationMethod.Rectangle
            ? SelectedStep?.Value ?? $"I = {Result.IntegralValue:G6}"
            : $"I = {Result.IntegralValue:G6}";

        await ExportPdfCoreAsync(
            methodName: $"Integration — {_method}",
            inputs: inputs,
            result: resultStr,
            steps: FilteredSteps,
            chartContainerId: IsChartVisible ? ChartContainerId : null,
            fileName: $"integration-{_method}.pdf",
            type: CalculationType.Integration);
    }
}
