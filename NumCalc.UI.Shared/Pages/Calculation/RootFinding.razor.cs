using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using NumCalc.Shared.Enums.RootFinding;
using NumCalc.Shared.RootFinding.Responses;
using NumCalc.UI.Shared.Components.RootFinding;
using NumCalc.UI.Shared.Enums.Roots;
using NumCalc.UI.Shared.HttpServices.Interfaces.Calculation;
using NumCalc.UI.Shared.Models.Charts;
using NumCalc.UI.Shared.Models.RootFinding;
using NumCalc.UI.Shared.Models.User.Enums;
using NumCalc.UI.Shared.Utils;
using NumCalc.UI.Shared.Utils.Calculation;

namespace NumCalc.UI.Shared.Pages.Calculation;

public partial class RootFinding : CalculationPage<RootFinding>
{
    private const string ChartContainerId = "chart--root-finding";

    [Inject] public IRootFindingApiService RootFindingApiService { get; set; } = null!;
    
    private RootFindingComparisonResponse? ComparisonResult { get; set; }
    private RootFindingResponse? Result { get; set; }
    private AnalysisMode Mode { get; set; }
    private bool IsChartVisible => !string.IsNullOrWhiteSpace(_formData.FunctionExpression);

    private List<RootFindingMethod> _benchmarkMethods = [];
    private readonly RootFindingFormData _formData = new();
    private RootFindingInput? _formDataInput;

    private async Task Calculate()
    {
        try
        {
            var (isValid, errorMessage) = await _formData.ValidateFormData(Mode, _benchmarkMethods, JsRuntime);
            if (!isValid)
            {
                UiService.ShowError(Localizer[errorMessage ?? "SomethingWentWrong"]);
                return;
            }

            if (Mode is AnalysisMode.Single) await DoSingleCalculation();
            else await DoBenchmarkCalculation();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Calculate failed");
            UiService.ShowError(Localizer["SomethingWentWrong"]);
        }
    }

    private async Task DoSingleCalculation()
    {
        var requestModel = _formData.GetSingleCalculationRequest();

        Func<Task<RootFindingResponse?>> apiCall = _formData.Method switch                                                                                                                                                                
        {
            RootFindingMethod.Dichotomy        => () => RootFindingApiService.GetDichotomyResultAsync(requestModel),
            RootFindingMethod.Newton           => () => RootFindingApiService.GetNewtonResultAsync(requestModel),
            RootFindingMethod.SimpleIterations => () => RootFindingApiService.GetSimpleIterationsResultAsync(requestModel),
            RootFindingMethod.Secant           => () => RootFindingApiService.GetSecantResultAsync(requestModel),
            RootFindingMethod.Combined         => () => RootFindingApiService.GetCombinedResultAsync(requestModel),
            _ => throw new ArgumentOutOfRangeException(nameof(_formData.Method))
        };

        Result = await SafeExecuteAsync(apiCall);

        if (Result is not null)
        {
            var historyRecord = _formData.GetHistoryRecord(Result);
            await TrySaveHistoryAsync(historyRecord);
        }

        await UpdateChart();
    }

    private async Task DoBenchmarkCalculation()
    {
        var request = _formData.GetComparisonRequest(_benchmarkMethods);
        ComparisonResult = await SafeExecuteAsync(() => RootFindingApiService.GetBenchmarkResultAsync(request));
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
        try
        {
            var asciiEquation = await _formDataInput?.GetAsciiExpressionAsync()!;
            if (string.IsNullOrWhiteSpace(asciiEquation)) return;
            if (_formData.StartPoint >= _formData.EndPoint) return;

            var config = RootFindingUtils.CreateChartConfig(
                ChartContainerId, 
                asciiEquation.NormalizeForChart(),
                _formData,
                Localizer);
            
            AppendResult(config);

            await JsRuntime.InvokeVoidAsync("NumCalc.drawPlot", config);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Chart update failed");
        }
    }

    private void AppendResult(Chart config)
    {
        if (Mode is AnalysisMode.Single && Result?.Root.HasValue == true)
        {
            config.AppendSingleResult(_formData, Result, Localizer);
        }
        else if (Mode is AnalysisMode.Benchmark && ComparisonResult?.Results is { Count: > 0 })
        {
            config.AppendBenchmarkResults(ComparisonResult, Localizer);
        }        
    }

    private async Task SaveInputAsync(string name)
    {
        await TrySaveInputAsync(name, CalculationType.RootFinding, JsonSerializer.Serialize(_formData));
    }

    private async Task LoadFromJsonAsync(string json)
    {
        var data = JsonSerializer.Deserialize<RootFindingFormData>(json);
        if (data is null) return;
        _formData.CopyFrom(data);
        StateHasChanged();

        await (_formDataInput?.SetLatexExpressionAsync(data.FunctionExpression) ?? Task.CompletedTask);
        await UpdateChart();
    }

    private async Task ExportPdfAsync()
    {
        if (Result is null) return;

        var inputs = _formData.GetPdfInputs();
        var resultStr = Result.Root.HasValue
            ? $"Root: {Result.Root.Value.FormatResult(_formData.Tolerance)}    Iterations: {Result.Iterations}"
            : "No root found";

        await ExportPdfCoreAsync(
            methodName: $"Root Finding — {_formData.Method}",
            inputs: inputs,
            result: resultStr,
            steps: Result.SolutionSteps,
            chartContainerId: IsChartVisible ? ChartContainerId : null,
            fileName: $"root-finding-{_formData.Method}.pdf",
            type: CalculationType.RootFinding);
    }
}