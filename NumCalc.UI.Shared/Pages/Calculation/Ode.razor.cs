using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NumCalc.Shared.ODE.Responses;
using NumCalc.UI.Shared.Components.ODE;
using NumCalc.UI.Shared.Enums.Roots;
using NumCalc.UI.Shared.HttpServices.Interfaces.Calculation;
using NumCalc.UI.Shared.Models.ODE;
using NumCalc.UI.Shared.Models.User.Enums;
using NumCalc.UI.Shared.Utils.Calculation;
using OdeMethod = NumCalc.Shared.Enums.ODE.OdeMethod;

namespace NumCalc.UI.Shared.Pages.Calculation;

public partial class Ode : CalculationPage<Ode>
{
    [Inject] private IOdeApiService OdeApiService { get; set; } = null!;

    private OdeResponse? Result { get; set; }
    private OdeComparisonResponse? ComparisonResult { get; set; }
    private bool IsChartVisible => Result?.SolutionPoints is { Count: > 0 };
    
    private AnalysisMode _mode = AnalysisMode.Single;
    private OdeMethod _method = OdeMethod.EulerImproved;
    private List<OdeMethod> _benchmarkMethods = [];
    private OdeInput? _input;

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
    
    private async Task DoSingleCalculation(OdeFormData formData)
    {
        var request = formData.GetSingleCalculationRequest();

        Func<Task<OdeResponse?>> apiCall = _method switch
        {
            OdeMethod.Euler         => () => OdeApiService.SolveEuler(request),
            OdeMethod.EulerImproved => () => OdeApiService.SolveEulerImproved(request),
            OdeMethod.RungeKutta2   => () => OdeApiService.SolveRungeKutta2(request),
            OdeMethod.RungeKutta4   => () => OdeApiService.SolveRungeKutta4(request),
            OdeMethod.Picard        => () => OdeApiService.SolvePicard(request),
            _ => throw new ArgumentOutOfRangeException(nameof(_method))
        };

        Result = await SafeExecuteAsync(apiCall);

        if (Result is null) return;

        var historyRecord = formData.GetHistoryRecord(Result, _method);
        await TrySaveHistoryAsync(historyRecord);
        await UpdateChart(formData);
    }

    private async Task DoBenchmarkCalculation(OdeFormData formData)
    {
        if (_mode is not AnalysisMode.Benchmark) return;

        if (_benchmarkMethods.Count == 0)
        {
            UiService.ShowError(Localizer["SelectAtLeastOneMethod"]);
            return;
        }

        var comparisonRequest = formData.GetComparisonRequest(_benchmarkMethods);
        ComparisonResult = await SafeExecuteAsync(() => OdeApiService.GetOdeComparisonAsync(comparisonRequest));
    }

    private async Task UpdateChart(OdeFormData formData)
    {
        if (Result?.SolutionPoints is not { Count: > 0 }) return;

        var chartConfig = formData.CreateChartConfig(Result, _method);
        if (chartConfig is null) return;
     
        await JsRuntime.InvokeVoidAsync("NumCalc.drawPlot", chartConfig);
    }

    private async Task SaveInputAsync(string name)
    {
        if (_input is null) return;
        var data = await _input.GetFormData();
        await TrySaveInputAsync(name, CalculationType.Ode, JsonSerializer.Serialize(data));
    }

    private async Task LoadFromJsonAsync(string json)
    {
        if (_input is null) return;
        var data = JsonSerializer.Deserialize<OdeFormData>(json);
        if (data is null) return;
        await _input.SetFormDataAsync(data);
    }

    private async Task ExportPdfAsync()
    {
        if (Result is null || _input is null) return;

        var formData = await _input.GetFormData();

        var inputs = formData.GetMethodInputs(_method);
        var resultSummary = Result.GetResultSummary();

        await ExportPdfCoreAsync(
            methodName: $"ODE — {_method}",
            inputs: inputs,
            result: resultSummary,
            steps: Result.SolutionSteps,
            chartContainerId: IsChartVisible ? OdeUtils.ChartContainerId : null,
            fileName: $"ode-{_method}.pdf",
            type: CalculationType.Ode);
    }
    
    private void ResetResult()
    {
        Result = null;
        ComparisonResult = null;
    }
}
