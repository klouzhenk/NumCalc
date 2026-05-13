using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NumCalc.Shared.Common;
using NumCalc.Shared.Enums.Integration;
using NumCalc.Shared.Integration.Responses;
using NumCalc.UI.Shared.Components.Integration;
using NumCalc.UI.Shared.Enums.Integration;
using NumCalc.UI.Shared.Enums.Roots;
using NumCalc.UI.Shared.HttpServices.Interfaces.Calculation;
using NumCalc.UI.Shared.Models.Integration;
using NumCalc.UI.Shared.Models.User.Enums;
using NumCalc.UI.Shared.Utils.Calculation;

namespace NumCalc.UI.Shared.Pages.Calculation;

public partial class Integration : CalculationPage<Integration>
{
    private const string ChartContainerId = "chart--integration";

    [Inject] private IIntegrationApiService IntegrationApiService { get; set; } = null!;
    
    private IntegrationResponse? Result { get; set; }
    private IntegrationComparisonResponse? ComparisonResult { get; set; }
    private bool IsChartVisible => Result?.ChartData is not null;
    private SolutionStep? SelectedStep => Result?.SolutionSteps?.FirstOrDefault();
    private IList<SolutionStep>? FilteredSteps => Result?.SolutionSteps;
    
    private AnalysisMode _mode = AnalysisMode.Single;
    private IntegrationMethod _method = IntegrationMethod.Rectangle;
    private RectangleVariant _rectangleVariant = RectangleVariant.Midpoint;
    private List<IntegrationComparisonMethod> _benchmarkMethods = [];
    private IntegrationInput? _input;

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

    private async Task DoSingleCalculation(IntegrationFormData formData)
    {
        if (_mode is not AnalysisMode.Single) return;
        
        var request = formData.GetSingleCalculationRequest(_method, _rectangleVariant);

        Func<Task<IntegrationResponse?>> apiCall = _method switch
        {
            IntegrationMethod.Rectangle => () => IntegrationApiService.IntegrateRectangleAsync(request),
            IntegrationMethod.Trapezoid => () => IntegrationApiService.IntegrateTrapezoidAsync(request),
            IntegrationMethod.Simpson   => () => IntegrationApiService.IntegrateSimpsonAsync(request),
            _ => throw new ArgumentOutOfRangeException(nameof(_method))
        };

        Result = await SafeExecuteAsync(apiCall);
        if (Result is null) return;

        var historyRecord = formData.GetHistoryRecord(Result, _method, _rectangleVariant);
        await TrySaveHistoryAsync(historyRecord);

        await UpdateChart(formData);
    }

    private async Task DoBenchmarkCalculation(IntegrationFormData formData)
    {
        if (_mode is not AnalysisMode.Benchmark) return;
        
        if (_benchmarkMethods.Count == 0)
        {
            UiService.ShowError(Localizer["SelectAtLeastOneMethod"]);
            return;
        }

        var comparisonRequest = formData.GetComparisonRequest(_benchmarkMethods);
        ComparisonResult = await SafeExecuteAsync(() => IntegrationApiService.GetIntegrationComparisonAsync(comparisonRequest));
    }

    private async Task UpdateChart(IntegrationFormData formData)
    {
        if (Result?.ChartData is null) return;

        var chartConfig = formData.CreateChartConfig(Result, _method, _rectangleVariant, ChartContainerId);
        if (chartConfig is null) return; 
        
        await JsRuntime.InvokeVoidAsync("NumCalc.drawPlot", chartConfig);
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
        if (Result is null || _input is null) return;

        var formData = await _input.GetFormData();

        var inputs = formData.GetMethodInputs(_method, _rectangleVariant);
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
    
    private void ResetResult()
    {
        Result = null;
        ComparisonResult = null;
    }
}
