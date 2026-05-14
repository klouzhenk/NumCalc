using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NumCalc.Shared.Enums.Optimization;
using NumCalc.Shared.Optimization.Responses;
using NumCalc.UI.Shared.Components.Optimization;
using NumCalc.UI.Shared.Enums.Optimization;
using NumCalc.UI.Shared.Enums.Roots;
using NumCalc.UI.Shared.HttpServices.Interfaces.Calculation;
using NumCalc.UI.Shared.Models.Optimization;
using NumCalc.UI.Shared.Models.User.Enums;
using NumCalc.UI.Shared.Utils.Calculation;

namespace NumCalc.UI.Shared.Pages.Calculation;

public partial class Optimization : CalculationPage<Optimization>
{
    [Inject] private IOptimizationApiService OptimizationApiService { get; set; } = null!;

    private OptimizationResponse? Result { get; set; }
    private OptimizationComparisonResponse? ComparisonResult { get; set; }
    private bool IsChartVisible => Result?.ChartData is not null
        || (_mode is AnalysisMode.Benchmark && ComparisonResult is not null);
    
    private AnalysisMode _mode = AnalysisMode.Single;
    private OptimizationMethod _method = OptimizationMethod.UniformSearch;
    private List<OptimizationComparisonMethod> _benchmarkMethods = [];
    private OptimizationInput? _input;
    private bool _maximize;

    private async Task Calculate()
    {
        if (_input is null) return;

        var formData = await _input.GetFormData();

        if (_mode is AnalysisMode.Single)
        {
            await DoSingleCalculation(formData);
            return;
        }

        await DoBenchmarkCalculation(formData);
    }

    private async Task DoBenchmarkCalculation(OptimizationFormData formData)
    {
        if (_mode is not AnalysisMode.Benchmark) return;
        
        if (_benchmarkMethods.Count == 0)
        {
            UiService.ShowError(Localizer["SelectAtLeastOneMethod"]);
            return;
        }

        var comparisonRequest = formData.GetComparisonRequest(_benchmarkMethods, _maximize);
        ComparisonResult = await SafeExecuteAsync(()
            => OptimizationApiService.GetOptimizationComparisonAsync(comparisonRequest));

        if (ComparisonResult is null) return;

        await UpdateBenchmarkChart(formData);
    }

    private async Task UpdateBenchmarkChart(OptimizationFormData formData)
    {
        var chartConfig = formData.CreateBenchmarkChartConfig(ComparisonResult!, Localizer);
        await JsRuntime.InvokeVoidAsync("NumCalc.drawPlot", chartConfig);
    }

    private async Task DoSingleCalculation(OptimizationFormData formData)
    {
        if (_mode is not AnalysisMode.Single) return;
        
        Func<Task<OptimizationResponse?>> apiCall = _method switch
        {
            OptimizationMethod.UniformSearch => 
                () => OptimizationApiService.OptimizeUniformSearchAsync(formData.GetOptimizationRequest(_maximize)),
            OptimizationMethod.GoldenSection => 
                () => OptimizationApiService.OptimizeGoldenSectionAsync(formData.GetOptimizationRequest(_maximize)),
            OptimizationMethod.GradientDescent => 
                () => OptimizationApiService.OptimizeGradientDescentAsync(formData.GetGradientRequest(_maximize)),
            _ => throw new ArgumentOutOfRangeException(nameof(_method))
        };

        Result = await SafeExecuteAsync(apiCall);

        if (Result is null) return;

        var historyRecord = formData.GetHistoryRecord(Result, _method, _maximize);
        await TrySaveHistoryAsync(historyRecord);
        await UpdateChart(formData);
    }

    private async Task UpdateChart(OptimizationFormData formData)
    {
        if (Result?.ChartData is null) return;

        var is3D = Result.ChartData.Any(p => p.Z.HasValue);

        if (is3D)
        {
            await Update3DChart(formData);
            return;
        }

        await Update2DChart(formData);
    }

    private async Task Update2DChart(OptimizationFormData formData)
    {
        var chartConfig = formData.Create2DChartConfig(Result!);
        if (chartConfig is null) return;
        await JsRuntime.InvokeVoidAsync("NumCalc.drawPlot", chartConfig);        
    }

    private async Task Update3DChart(OptimizationFormData formData)
    {
        var chartConfig = formData.Create3DChartConfig(Result!);
        if (chartConfig is null) return;
        await JsRuntime.InvokeVoidAsync("NumCalc.drawPlot3d", chartConfig);
    }

    private async Task SaveInputAsync(string name)
    {
        if (_input is null) return;
        var data = await _input.GetFormData();
        data.Maximize = _maximize;
        data.AnalysisMode = _mode;
        data.BenchmarkMethods = _benchmarkMethods;
        await TrySaveInputAsync(name, CalculationType.Optimization, JsonSerializer.Serialize(data));
    }

    private async Task LoadFromJsonAsync(string json)
    {
        if (_input is null) return;
        var data = JsonSerializer.Deserialize<OptimizationFormData>(json);
        if (data is null) return;
        _method = data.Method;
        _maximize = data.Maximize;
        _mode = data.AnalysisMode;
        _benchmarkMethods = data.BenchmarkMethods;
        StateHasChanged();
        await Task.Yield();
        await _input.SetFormDataAsync(data);
    }

    private async Task ExportPdfAsync()
    {
        if (Result is null || _input is null) return;

        var formData = await _input.GetFormData();

        var inputs = formData.GetMethodInputs(_method, _maximize);
        var resultSummary = Result.GetResultSummary(formData);

        await ExportPdfCoreAsync(
            methodName: $"Optimization — {_method}",
            inputs: inputs,
            result: resultSummary,
            steps: Result.SolutionSteps,
            chartContainerId: IsChartVisible ? OptimizationUtils.ChartContainerId : null,
            fileName: $"optimization-{_method}.pdf",
            type: CalculationType.Optimization);
    }
    
    private void ResetResult()
    {
        Result = null;
        ComparisonResult = null;
    }
}
