using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using NumCalc.Shared.Differentiation.Responses;
using NumCalc.Shared.Enums.Differentiation;
using NumCalc.UI.Shared.Components.Differentiation;
using NumCalc.UI.Shared.Enums.Differentiation;
using NumCalc.UI.Shared.Enums.Roots;
using NumCalc.UI.Shared.HttpServices.Interfaces.Calculation;
using NumCalc.UI.Shared.Models.Differentiation;
using NumCalc.UI.Shared.Models.User.Enums;
using NumCalc.UI.Shared.Utils.Calculation;
using FiniteDiffVariant = NumCalc.Shared.Enums.Differentiation.FiniteDiffVariant;

namespace NumCalc.UI.Shared.Pages.Calculation;

public partial class Differentiation : CalculationPage<Differentiation>
{
    [Inject] private IDifferentiationApiService DifferentiationApiService { get; set; } = null!;

    private DifferentiationResponse? Result { get; set; }
    private DifferentiationComparisonResponse? ComparisonResult { get; set; }
    private DifferentiationFormData FormData { get; set; } = new();
    private bool IsChartVisible => Result?.ChartData is not null
        || (_mode is AnalysisMode.Benchmark && ComparisonResult is not null);
    
    private AnalysisMode _mode = AnalysisMode.Single;
    private DifferentiationMethod _method = DifferentiationMethod.FiniteDifferences;
    private FiniteDiffVariant _variant = FiniteDiffVariant.Central;
    private DifferentiationInputMode _inputMode = DifferentiationInputMode.Function;
    private List<DifferentiationComparisonMethod> _benchmarkMethods = [];
    private DifferentiationInput? _input;

    private async Task Calculate()
    {
        try
        {
            if (_input is null) return;

            FormData = await _input.GetFormData();

            var (isValid, errorMessage) = FormData.ValidateFormData();
            if (!isValid)
            {
                UiService.ShowError(Localizer[errorMessage ?? "SomethingWentWrong"]);
                return;
            }

            if (_mode is AnalysisMode.Single)
            {
                await DoSingleCalculation();
                return;
            }

            await DoBenchmarkCalculation();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while calculating");
            UiService.ShowError(Localizer["SomethingWentWrong"]);
        }
    }
    
    private async Task DoSingleCalculation()
    {
        if (_mode is not AnalysisMode.Single) return;
        
        var request = FormData.GetSingleCalculationRequest();

        Func<Task<DifferentiationResponse?>> apiCall = _method switch
        {
            DifferentiationMethod.FiniteDifferences => () => DifferentiationApiService.DifferentiateFiniteDiffAsync(request, _variant),
            DifferentiationMethod.Lagrange           => () => DifferentiationApiService.DifferentiateLagrangeAsync(request),
            _ => throw new ArgumentOutOfRangeException(nameof(_method))
        };

        Result = await SafeExecuteAsync(apiCall);
        
        if (Result is null) return;

        var historyRecord = FormData.GetHistoryRecord(Result, _method, _variant);
        await TrySaveHistoryAsync(historyRecord);
        await UpdateChart();
    }

    private async Task DoBenchmarkCalculation()
    {
        if (_mode is not AnalysisMode.Benchmark)
            return;
        
        if (_benchmarkMethods.Count == 0)
        {
            UiService.ShowError(Localizer["SelectAtLeastOneMethod"]);
            return;
        }

        var comparisonRequest = FormData.GetComparisonRequest(_benchmarkMethods);
        ComparisonResult = await SafeExecuteAsync(()
            => DifferentiationApiService.GetDifferentiationComparisonAsync(comparisonRequest));

        if (ComparisonResult is null) return;

        await UpdateBenchmarkChart();
    }

    private async Task UpdateChart()
    {
        var config = FormData.CreateChartConfig(Result);
        if (config is null) return;
        await JsRuntime.InvokeVoidAsync("NumCalc.drawPlot", config);
    }

    private async Task UpdateBenchmarkChart()
    {
        var config = FormData.CreateBenchmarkChartConfig(ComparisonResult!, Localizer);
        if (config is null) return;
        await JsRuntime.InvokeVoidAsync("NumCalc.drawPlot", config);
    }

    private async Task SaveInputAsync(string name)
    {
        if (_input is null) return;
        var data = await _input.GetFormData();
        data.AnalysisMode = _mode;
        data.Method = _method;
        data.Variant = _variant;
        data.BenchmarkMethods = _benchmarkMethods;
        await TrySaveInputAsync(name, CalculationType.Differentiation, JsonSerializer.Serialize(data));
    }

    private async Task LoadFromJsonAsync(string json)
    {
        if (_input is null) return;
        var data = JsonSerializer.Deserialize<DifferentiationFormData>(json);
        if (data is null) return;
        _inputMode = data.Mode;
        _mode = data.AnalysisMode;
        _method = data.Method;
        _variant = data.Variant;
        _benchmarkMethods = data.BenchmarkMethods;
        StateHasChanged();
        await Task.Yield();
        await _input.SetFormDataAsync(data);
    }

    private async Task ExportPdfAsync()
    {
        if (Result is null) return;

        var (inputs, methodLabel) = 
            FormData.GetMethodInputs(_method, _variant);

        var order = FormData.DerivativeOrder == 2 ? "f''" : "f'";
        var resultStr = $"{order}(x*) = {Result.DerivativeValue:G10}";

        await ExportPdfCoreAsync(
            methodName: $"Differentiation — {methodLabel}",
            inputs: inputs,
            result: resultStr,
            steps: Result.SolutionSteps,
            chartContainerId: IsChartVisible ? DifferentiationUtils.ChartContainerId : null,
            fileName: $"differentiation-{_method}.pdf",
            type: CalculationType.Differentiation);
    }
    
    private void ResetResult()
    {
        Result = null;
        ComparisonResult = null;
    }
}
