using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using NumCalc.Shared.Interpolation.Responses;
using NumCalc.UI.Shared.Components.Interpolation;
using NumCalc.UI.Shared.Enums.Roots;
using NumCalc.UI.Shared.HttpServices.Interfaces.Calculation;
using NumCalc.UI.Shared.Models.Interpolation;
using NumCalc.UI.Shared.Models.User.Enums;
using NumCalc.UI.Shared.Utils.Calculation;
using InterpolationMethod = NumCalc.Shared.Enums.Interpolation.InterpolationMethod;

namespace NumCalc.UI.Shared.Pages.Calculation;

public partial class Interpolation : CalculationPage<Interpolation>
{
    [Inject] private IInterpolationApiService InterpolationApiService { get; set; } = null!;
    
    private InterpolationResponse? Result { get; set; }
    private InterpolationComparisonResponse? ComparisonResult { get; set; }
    private bool IsChartVisible => Result?.ChartData is not null
        || (_analysisMode is AnalysisMode.Benchmark && ComparisonResult is not null);
    
    private InterpolationMethod _method = InterpolationMethod.Newton;
    private InterpolationInput? _input;
    private AnalysisMode _analysisMode = AnalysisMode.Single;
    private List<InterpolationMethod> _benchmarkMethods = [];
    
    private async Task Calculate()
    {
        try
        {
            if (_input is null) return;
            var formData = await _input.GetFormData();

            if (_analysisMode is AnalysisMode.Single)
            {
                await DoSingleCalculation(formData);
                return;
            }

            await DoBenchmarkCalculation(formData);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Calculate failed");
            UiService.ShowError(Localizer["SomethingWentWrong"]);
        }
    }

    private async Task DoSingleCalculation(InterpolationFormData formData)
    {
        var request = formData.GetSingleCalculationRequest();

        Func<Task<InterpolationResponse?>> apiCall = _method switch
        {
            InterpolationMethod.Newton   => () => InterpolationApiService.InterpolateNewtonAsync(request),
            InterpolationMethod.Lagrange => () => InterpolationApiService.InterpolateLagrangeAsync(request),
            InterpolationMethod.Spline   => () => InterpolationApiService.InterpolateSplineAsync(request),
            _ => throw new ArgumentOutOfRangeException(nameof(_method))
        };

        Result = await SafeExecuteAsync(apiCall);

        if (Result is null) return;

        var historyRecord = formData.GetHistoryRecord(Result, _method);
        await TrySaveHistoryAsync(historyRecord);
        await UpdateChart(formData);
    }

    private async Task DoBenchmarkCalculation(InterpolationFormData formData)
    {
        if (_analysisMode is not AnalysisMode.Benchmark) return;
        
        if (_benchmarkMethods.Count == 0)
        {
            UiService.ShowError(Localizer["SelectAtLeastOneMethod"]);
            return;
        }

        var comparisonRequest = formData.GetComparisonRequest(_benchmarkMethods);
        ComparisonResult = await SafeExecuteAsync(() =>
            InterpolationApiService.GetInterpolationComparisonAsync(comparisonRequest));

        if (ComparisonResult is null) return;

        await UpdateBenchmarkChart(formData);
    }

    private async Task UpdateBenchmarkChart(InterpolationFormData formData)
    {
        var chartConfig = formData.CreateBenchmarkChartConfig(ComparisonResult!, Localizer);
        await JsRuntime.InvokeVoidAsync("NumCalc.drawPlot", chartConfig);
    }

    private async Task UpdateChart(InterpolationFormData formData)
    {
        if (Result?.ChartData is null) return;
        
        var chartConfig = formData.CreateChartConfig(Result, _method, Localizer);
        await JsRuntime.InvokeVoidAsync("NumCalc.drawPlot", chartConfig);
    }

    private async Task SaveInputAsync(string name)
    {
        if (_input is null) return;
        var data = await _input.GetFormData();
        data.AnalysisMode = _analysisMode;
        data.Method = _method;
        data.BenchmarkMethods = _benchmarkMethods;
        await TrySaveInputAsync(name, CalculationType.Interpolation, JsonSerializer.Serialize(data));
    }

    private async Task LoadFromJsonAsync(string json)
    {
        if (_input is null) return;
        var data = JsonSerializer.Deserialize<InterpolationFormData>(json);
        if (data is null) return;
        _analysisMode = data.AnalysisMode;
        _method = data.Method;
        _benchmarkMethods = data.BenchmarkMethods;
        StateHasChanged();
        await Task.Yield();
        await _input.SetFormDataAsync(data);
    }

    private async Task ExportPdfAsync()
    {
        if (Result is null || _input is null) return;
        
        var formData = await _input.GetFormData();
        var inputs = formData.GetMethodInputs(_method);

        await ExportPdfCoreAsync(
            methodName: $"Interpolation — {_method}",
            inputs: inputs,
            result: $"P(x*) = {Result.InterpolatedValue:G6}",
            steps: Result.SolutionSteps,
            chartContainerId: IsChartVisible ? InterpolationUtils.ChartContainerId : null,
            fileName: $"interpolation-{_method}.pdf",
            type: CalculationType.Interpolation);
    }
    
    private void ResetResult()
    {
        Result = null;
        ComparisonResult = null;
    }
}
