using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NumCalc.Shared.Enums.Interpolation;
using NumCalc.Shared.Interpolation.Requests;
using NumCalc.Shared.Interpolation.Responses;
using NumCalc.UI.Shared.Components.Interpolation;
using NumCalc.UI.Shared.Enums;
using NumCalc.UI.Shared.Enums.Charts;
using NumCalc.UI.Shared.Enums.Roots;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.Charts;
using NumCalc.UI.Shared.Models.Interpolation;
using NumCalc.UI.Shared.Models.User;
using NumCalc.UI.Shared.Models.User.Enums;
using NumCalc.UI.Shared.Utils;
using InterpolationMethod = NumCalc.Shared.Enums.Interpolation.InterpolationMethod;

namespace NumCalc.UI.Shared.Pages.Calculation;

public partial class Interpolation : CalculationPage<Interpolation>
{
    private const string ChartContainerId = "chart--interpolation";

    [Inject] private ICalculationApiService CalculationApiService { get; set; } = null!;

    private InterpolationMethod _method = InterpolationMethod.Newton;
    private InterpolationInput? _input;
    private AnalysisMode _analysisMode = AnalysisMode.Single;
    private List<InterpolationMethod> _benchmarkMethods = [];
    private InterpolationResponse? Result { get; set; }
    private InterpolationComparisonResponse? ComparisonResult { get; set; }
    
    private bool IsChartVisible => Result?.ChartData is not null;
    
    private void ResetResult()
    {
        Result = null;
        ComparisonResult = null;
    }

    private async Task Calculate()
    {
        Result = null;

        if (_input is null) return;

        var formData = await _input.GetFormData();
        
        if (_analysisMode is AnalysisMode.Benchmark)
        {
            if (_benchmarkMethods.Count == 0)
            {
                UiService.ShowError(Localizer["SelectAtLeastOneMethod"]);
                return;
            }

            var comparisonRequest = new InterpolationComparisonRequest()
            {
                FunctionExpression = formData.FunctionExpression ?? string.Empty,
                XNodes = formData.XNodes,
                YValues = formData.YValues,
                QueryPoint = formData.QueryPoint,
                Methods = _benchmarkMethods
            };

            ComparisonResult = await SafeExecuteAsync(() => CalculationApiService.GetInterpolationComparisonAsync(comparisonRequest));
            return;
        }

        var request = new InterpolationRequest
        {
            Mode = formData.Mode,
            FunctionExpression = formData.FunctionExpression,
            XNodes = formData.XNodes,
            YValues = formData.YValues,
            QueryPoint = formData.QueryPoint
        };

        Func<Task<InterpolationResponse?>> apiCall = _method switch
        {
            InterpolationMethod.Newton   => () => CalculationApiService.InterpolateNewtonAsync(request),
            InterpolationMethod.Lagrange => () => CalculationApiService.InterpolateLagrangeAsync(request),
            InterpolationMethod.Spline   => () => CalculationApiService.InterpolateSplineAsync(request),
            _ => throw new ArgumentOutOfRangeException(nameof(_method))
        };

        Result = await SafeExecuteAsync(apiCall);

        if (Result is not null)
        {
            var inputs = new Dictionary<string, string>
            {
                ["Method"] = _method.ToString(),
                ["Mode"] = formData.Mode.ToString(),
                ["Query Point"] = formData.QueryPoint.ToString("G")
            };
            if (!string.IsNullOrWhiteSpace(formData.FunctionExpression))
                inputs["Expression"] = formData.FunctionExpression;
            if (formData.XNodes is { Count: > 0 })
                inputs["X Nodes"] = string.Join(", ", formData.XNodes);

            await TrySaveHistoryAsync(new SaveCalculationRecordRequest
            {
                Type = CalculationType.Interpolation,
                MethodName = _method.ToString(),
                InputsJson = JsonSerializer.Serialize(inputs),
                ResultSummary = $"P(x*) = {Result.InterpolatedValue:G10}",
                ExecutionTimeMs = Result.ExecutionTimeMs
            });

            await UpdateChart(formData);
        }
    }

    private async Task UpdateChart(InterpolationFormData data)
    {
        if (Result?.ChartData is null) return;

        var chartData = Result.ChartData
            .Where(p => p is { X: not null, Y: not null })
            .Select(p => new[] { p.X!.Value, p.Y!.Value })
            .ToList();

        var config = new Chart
        {
            ContainerId = ChartContainerId,
            Title = null,
            XAxis = new ChartAxis
            {
                Title = Localizer["ArgumentX"],
                PlotLines = [ChartUtils.CreateZeroLine()]
            },
            YAxis = new ChartAxis
            {
                Title = Localizer["FunctionValue"],
                PlotLines = [ChartUtils.CreateZeroLine()]
            },
            Series =
            [
                new ChartSeries
                {
                    Name = _method.ToString(),
                    Data = chartData,
                    Color = ColorUtils.GetColor(Color.Primary),
                    LineWidth = 2,
                    IsVisible = true
                },
                new ChartSeries
                {
                    Name = "x*",
                    Type = ChartType.Scatter,
                    Data = [[data.QueryPoint, Result.InterpolatedValue]],
                    Color = ColorUtils.GetColor(Color.PrimaryDark),
                    IsVisible = true,
                    Marker = new ChartMarker { Radius = 8, Symbol = ChartSymbolType.Circle }
                }
            ]
        };

        await JsRuntime.InvokeVoidAsync("NumCalc.drawPlot", config);
    }

    private async Task SaveInputAsync(string name)
    {
        if (_input is null) return;
        var data = await _input.GetFormData();
        await TrySaveInputAsync(name, CalculationType.Interpolation, JsonSerializer.Serialize(data));
    }

    private async Task LoadFromJsonAsync(string json)
    {
        if (_input is null) return;
        var data = JsonSerializer.Deserialize<InterpolationFormData>(json);
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
            ["Mode"] = formData.Mode.ToString(),
            ["Query Point"] = formData.QueryPoint.ToString("G")
        };
        if (formData.Mode is InterpolationInputMode.Function && !string.IsNullOrWhiteSpace(formData.FunctionExpression))
            inputs["Expression"] = formData.FunctionExpression;
        if (formData.XNodes is { Count: > 0 })
            inputs["X Nodes"] = string.Join(", ", formData.XNodes);

        await ExportPdfCoreAsync(
            methodName: $"Interpolation — {_method}",
            inputs: inputs,
            result: $"P(x*) = {Result.InterpolatedValue:G6}",
            steps: Result.SolutionSteps,
            chartContainerId: IsChartVisible ? ChartContainerId : null,
            fileName: $"interpolation-{_method}.pdf",
            type: CalculationType.Interpolation);
    }
}
