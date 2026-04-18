using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NumCalc.Shared.Enums.RootFinding;
using NumCalc.Shared.RootFinding.Requests;
using NumCalc.Shared.RootFinding.Responses;
using NumCalc.UI.Shared.Components;
using NumCalc.UI.Shared.Enums;
using NumCalc.UI.Shared.Enums.Charts;
using NumCalc.UI.Shared.Enums.Roots;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.Charts;
using NumCalc.UI.Shared.Models.Export;
using NumCalc.UI.Shared.Models.RootFinding;
using NumCalc.UI.Shared.Services.Interfaces;
using NumCalc.UI.Shared.Utils;

namespace NumCalc.UI.Shared.Pages;

public partial class RootFinding : BasePage<RootFinding>
{
    private const string ChartContainerId = "chart--root-finding";

    [Inject] public ICalculationApiService CalculationApiService { get; set; } = null!;
    [Inject] public IPdfExportService PdfExportService { get; set; } = null!;
    
    private AnalysisMode Mode { get; set; }
    private List<RootFindingMethod> _benchmarkMethods = [];
    
    private readonly RootFindingFormData _formData = new();
    
    private RootFindingResponse? Result { get; set; }
    private RootFindingComparisonResponse? ComparisonResult { get; set; }
    private MathInput? _mathInputComponent;
    private bool IsChartVisible => !string.IsNullOrWhiteSpace(_formData.FunctionExpression);

    private record ExpressionValidationResult(bool Valid, string[] Variables);

    private async Task Calculate()
    {
        Result = null;
        ComparisonResult = null;

        if (!await ValidateAsync()) return;

        if (Mode == AnalysisMode.Single) await DoSingleMethodCalculation();
        else await DoMultipleMethodCalculations();
    }

    private async Task<bool> ValidateAsync()
    {
        if (string.IsNullOrWhiteSpace(_formData.FunctionExpression))
        {
            UiService.ShowError(Localizer["ExpressionRequired"]);
            return false;
        }

        // TODO : check the bug with '5x + 3'
        // var result = await JsRuntime.InvokeAsync<ExpressionValidationResult>(
        //     "NumCalc.validateExpression", _formData.FunctionExpression);
        //
        // if (!result.Valid)
        // {
        //     UiService.ShowError(Localizer["ExpressionInvalid"]);
        //     return false;
        // }

        // if (result.Variables.Any(v => v != "x"))
        // {
        //     UiService.ShowError(Localizer["ExpressionOnlyX"]);
        //     return false;
        // }

        var isNewton = Mode is AnalysisMode.Single && _formData.Method is RootFindingMethod.Newton;
        if (!isNewton && _formData.StartPoint >= _formData.EndPoint)
        {
            UiService.ShowError(Localizer["StartMustBeLessThanEnd"]);
            return false;
        }

        if (Mode is AnalysisMode.Benchmark && _benchmarkMethods.Count == 0)
        {
            UiService.ShowError(Localizer["SelectAtLeastOneMethod"]);
            return false;
        }

        return true;
    }

    private async Task DoSingleMethodCalculation()
    {
        var requestModel = new RootFindingRequest()
        {
            FunctionExpression = _formData.FunctionExpression ?? string.Empty,
            StartRange = _formData.StartPoint,
            EndRange = _formData.EndPoint,
            Error = _formData.Tolerance
        };

        Func<Task<RootFindingResponse?>> apiCall = _formData.Method switch                                                                                                                                                                
        {
            RootFindingMethod.Dichotomy        => () => CalculationApiService.GetDichotomyResultAsync(requestModel),
            RootFindingMethod.Newton           => () => CalculationApiService.GetNewtonResultAsync(requestModel),
            RootFindingMethod.SimpleIterations => () => CalculationApiService.GetSimpleIterationsResultAsync(requestModel),
            RootFindingMethod.Secant           => () => CalculationApiService.GetSecantResultAsync(requestModel),
            RootFindingMethod.Combined         => () => CalculationApiService.GetCombinedResultAsync(requestModel),
            _ => throw new ArgumentOutOfRangeException(nameof(_formData.Method))
        };

        Result = await SafeExecuteAsync(apiCall);
        
        await UpdateChart();
    }

    private async Task DoMultipleMethodCalculations()
    {
        var request = new RootFindingComparisonRequest()
        {
            FunctionExpression = _formData.FunctionExpression ?? string.Empty,
            StartRange = _formData.StartPoint,
            EndRange = _formData.EndPoint,
            Tolerance = _formData.Tolerance,
            Methods = _benchmarkMethods
        };

        ComparisonResult = await SafeExecuteAsync(() => CalculationApiService.GetBenchmarkResultAsync(request));

        await UpdateChart();
    }

    private async Task OnParametersChanged()
    {
        Result = null;
        ComparisonResult = null;
        await UpdateChart();
    }

    private async Task UpdateChart()
    {
        var asciiEquation = _mathInputComponent is not null
            ? await _mathInputComponent.GetAsciiValue()
            : null;
        if (string.IsNullOrWhiteSpace(asciiEquation)) return;
        if (_formData.StartPoint >= _formData.EndPoint) return;

        var config = CreateChartConfig(asciiEquation.NormalizeForChart(), _formData.StartPoint, _formData.EndPoint);
        AppendResultSeries(config);

        await JsRuntime.InvokeVoidAsync("NumCalc.drawPlot", config);
    }

    private void AppendResultSeries(Chart config)
    {
        if (Mode is AnalysisMode.Single && Result?.Root.HasValue == true)
        {
            config.Series.Add(new ChartSeries
            {
                Name = $"{Localizer["Root"]} ({_formData.Method})",
                Type = ChartType.Scatter,
                Data = [[Result.Root.Value, 0]],
                Color = ColorUtils.GetColor(Color.SuccessLight),
                IsVisible = true
            });
        }
        else if (Mode is AnalysisMode.Benchmark && ComparisonResult?.Results is { Count: > 0 })
        {
            foreach (var result in ComparisonResult.Results)
            {
                config.Series.Add(new ChartSeries
                {
                    Name = Localizer[result.Method.ToString()],
                    Type = ChartType.Scatter,
                    Data = result.Root.HasValue ? [[result.Root.Value, 0]] : null,
                    Color = ColorUtils.GetSeriesColor((int)result.Method),
                    IsVisible = true,
                    Marker = new ChartMarker { Radius = 5, Symbol = ChartSymbolType.Circle },
                    Opacity = 0.8
                });
            }
        }
    }
    
    private Chart CreateChartConfig(string expression, double min, double max)
    {
        return new Chart
        {
            ContainerId = ChartContainerId,
            Title = null,
            XAxis = new ChartAxis
            {
                Min = min,
                Max = max,
                Title = Localizer["ArgumentX"],
                PlotLines = [ ChartUtils.CreateZeroLine() ]
            },

            YAxis = new ChartAxis
            {
                Title = Localizer["FunctionValue"],
                PlotLines = [ ChartUtils.CreateZeroLine() ]
            },

            Series =
            [
                new ChartSeries
                {
                    Name = "f(x)",
                    Expression = expression,
                    Color = ColorUtils.GetColor(Color.Primary),
                    LineWidth = 2,
                    IsVisible = true
                }
            ]
        };
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

            steps.Add(new StepExportItem
            {
                Description = step.Description,
                ImageBase64 = imageBase64,
                Value = step.Value
            });
        }

        var chartImage = IsChartVisible
            ? await JsRuntime.InvokeAsync<string>("PdfHelper.getChartImage", ChartContainerId)
            : null;

        var isNewton = _formData.Method is RootFindingMethod.Newton;
        var inputs = new Dictionary<string, string>
        {
            ["Method"] = _formData.Method.ToString(),
            ["Expression"] = _formData.FunctionExpression ?? string.Empty,
            [isNewton ? "Initial Guess" : "Start"] = _formData.StartPoint.ToString("G"),
        };
        if (!isNewton) inputs["End"] = _formData.EndPoint.ToString("G");
        inputs["Tolerance"] = _formData.Tolerance.ToString("G");

        var request = new PdfExportRequest
        {
            MethodName = $"Root Finding — {_formData.Method}",
            Inputs = inputs,
            Result = Result.Root.HasValue
                ? $"Root: {Result.Root.Value:G10}    Iterations: {Result.Iterations}"
                : "No root found",
            Steps = steps,
            ChartImage = chartImage
        };

        var pdfBytes = PdfExportService.GeneratePdf(request);
        var base64 = Convert.ToBase64String(pdfBytes);
        await JsRuntime.InvokeVoidAsync("PdfHelper.downloadFile",
            $"root-finding-{_formData.Method}.pdf", "application/pdf", base64);
    }
}