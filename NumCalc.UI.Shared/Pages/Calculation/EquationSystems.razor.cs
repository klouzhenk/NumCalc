using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NumCalc.Shared.Enums.EquationSystems;
using NumCalc.Shared.EquationsSystems.Responses;
using NumCalc.UI.Shared.Components;
using NumCalc.UI.Shared.Components.EquationSystems;
using NumCalc.UI.Shared.Enums.EquationSystems;
using NumCalc.UI.Shared.Enums.Roots;
using NumCalc.UI.Shared.HttpServices.Interfaces.Calculation;
using NumCalc.UI.Shared.Models.EquationSystems;
using NumCalc.UI.Shared.Models.User.Enums;
using NumCalc.UI.Shared.Utils.Calculation;

namespace NumCalc.UI.Shared.Pages.Calculation;

public partial class EquationSystems : CalculationPage<EquationSystems>
{
    [Inject] private IEquationSystemApiService EquationSystemApiService { get; set; } = null!;
    
    private EquationSystemCategory Category { get; set; } = EquationSystemCategory.Linear;
    private LinearSystemMethod LinearMethod { get; set; } = LinearSystemMethod.Cramer;
    private NonLinearSystemMethod NonLinearMethod { get; set; } = NonLinearSystemMethod.FixedPoint;
    private LinearSystemComparisonResponse? LinearComparisonResult { get; set; }
    private NonLinearSystemComparisonResponse? NonLinearComparisonResult { get; set; }
    private SystemSolvingResponse? Result { get; set; }
    private int Size { get; set; } = 2;
    
    private bool IsChartVisible => Result?.ChartSeries is { Count: > 0 };

    private AnalysisMode _mode = AnalysisMode.Single;
    private readonly int[] _sizes = [2, 3, 4];
    private List<LinearSystemMethod>? _linearBenchmarkMethods;
    private List<NonLinearSystemMethod>? _nonLinearBenchmarkMethods;
    
    private LinearSystemInput? _linearInput;
    private EquationList? _equationList;

    private async Task Calculate()
    {
        if (_mode is AnalysisMode.Single)
        {
            await DoSingleCalculation();
            return;
        }

        await DoBenchmarkCalculation();
    }

    private async Task DoSingleCalculation()
    {
        if (Category is EquationSystemCategory.Linear)
        {
            await DoSingleLinearCalculation();
            return;
        }
        
        await DoSingleNonLinearCalculation();
    }

    private async Task DoBenchmarkCalculation()
    {
        if (Category is EquationSystemCategory.Linear)
        {
            await DoBenchmarkLinearCalculation();
            return;
        }
        
        await DoBenchmarkNonLinearCalculation();
    }

    private async Task DoBenchmarkLinearCalculation()
    {
        if (_linearInput is null) return;
                
        if (_linearBenchmarkMethods is not { Count: > 0 })
        {
            UiService.ShowError(Localizer["SelectAtLeastOneMethod"]);
            return;
        }

        var request = _linearInput.GetLinearComparisonRequest(Size, _linearBenchmarkMethods);

        LinearComparisonResult = await SafeExecuteAsync(() 
            => EquationSystemApiService.GetLinearComparisonAsync(request));
    }

    private async Task DoBenchmarkNonLinearCalculation()
    {
        if (_equationList is null) return;
        
        if (_nonLinearBenchmarkMethods is not { Count: > 0 })
        {
            UiService.ShowError(Localizer["SelectAtLeastOneMethod"]);
            return;
        }
        
        var formData = await _equationList.GetFormData();
        if (formData.IterationFunctions.Any(string.IsNullOrWhiteSpace))
        {
            UiService.ShowError(Localizer["ExpressionRequired"]);
            return;
        }

        var request = formData.GetNonLinearComparisonRequest(_nonLinearBenchmarkMethods);
        NonLinearComparisonResult = await SafeExecuteAsync(() 
            => EquationSystemApiService.GetNonLinearComparisonAsync(request));
    }

    private async Task DoSingleLinearCalculation()
    {
        if (_linearInput is null) return;

        var (request, variables, equations) = _linearInput.GetLinearCalculationRequest(Size);
        Func<Task<SystemSolvingResponse?>> apiCall = LinearMethod switch
        {
            LinearSystemMethod.Cramer => () => EquationSystemApiService.SolveCramerAsync(request),
            LinearSystemMethod.Gauss  => () => EquationSystemApiService.SolveGaussianAsync(request),
            _ => throw new ArgumentOutOfRangeException(nameof(LinearMethod))
        };

        Result = await SafeExecuteAsync(apiCall);

        if (Result is null) return;

        var historyRecord = Result.GetLinearHistoryRecord(variables, equations, LinearMethod);
        await TrySaveHistoryAsync(historyRecord);
        await UpdateChart();
    }

    private async Task DoSingleNonLinearCalculation()
    {
        if (_equationList is null) return;

        var formData = await _equationList.GetFormData();

        if (formData.IterationFunctions.Any(string.IsNullOrWhiteSpace))
        {
            UiService.ShowError(Localizer["ExpressionRequired"]);
            return;
        }

        var request = formData.GetNonLinearCalculationRequest();
        Func<Task<SystemSolvingResponse?>> apiCall = NonLinearMethod switch
        {
            NonLinearSystemMethod.FixedPoint => () => EquationSystemApiService.SolveFixedPointAsync(request),
            NonLinearSystemMethod.Seidel     => () => EquationSystemApiService.SolveSeidelAsync(request),
            _ => throw new ArgumentOutOfRangeException(nameof(NonLinearMethod))
        };

        Result = await SafeExecuteAsync(apiCall);
        if (Result is null) return;

        var historyRecord = Result.GetNonLinearHistoryRecord(formData, NonLinearMethod);
        await TrySaveHistoryAsync(historyRecord);
        await UpdateChart();
    }
    
    private async Task UpdateChart()
    {
        if (Result?.ChartSeries is not { Count: > 0 }) return;

        var nonLinearFormData = Category is EquationSystemCategory.NonLinear && _equationList is not null
            ? await _equationList.GetFormData()
            : null;

        var is3D = Result.ChartSeries.Any(s => s.Points.Any(p => p.Z.HasValue));

        if (is3D)
        {
            await Update3DChart(nonLinearFormData);
            return;
        }
        
        await Update2DChart(nonLinearFormData);
    }
    
    private async Task Update2DChart(NonLinearSystemFormData? nonLinearFormData)
    {
        var config = Result!.Create2DChartConfig(nonLinearFormData, Category, Size);
        if (config is null) return;
        await JsRuntime.InvokeVoidAsync("NumCalc.drawPlot", config);
    }

    private async Task Update3DChart(NonLinearSystemFormData? nonLinearFormData)
    {
        var config = Result!.Create3DChartConfig(nonLinearFormData, Category, Size);
        if (config is null) return;
        await JsRuntime.InvokeVoidAsync("NumCalc.drawPlot3d", config);
    }

    private async Task SaveInputAsync(string name)
    {
        string? json = null;

        if (Category is EquationSystemCategory.Linear && _linearInput is not null)
            json = _linearInput.GetLinearInputSaveData(_mode, LinearMethod, _linearBenchmarkMethods);
        else if (_equationList is not null)
            json = await _equationList.GetNonLinearInputSaveData(_mode, NonLinearMethod, _nonLinearBenchmarkMethods);

        if (json is null) return;
        await TrySaveInputAsync(name, CalculationType.EquationSystems, json);
    }

    private async Task LoadFromJsonAsync(string json)
    {
        var data = JsonSerializer.Deserialize<EquationSystemsSaveData>(json);
        if (data is null) return;

        Category = data.Category;
        Size = data.Size;
        _mode = data.AnalysisMode;
        LinearMethod = data.LinearMethod;
        NonLinearMethod = data.NonLinearMethod;
        _linearBenchmarkMethods = data.LinearBenchmarkMethods;
        _nonLinearBenchmarkMethods = data.NonLinearBenchmarkMethods;

        StateHasChanged();
        await Task.Yield(); // wait for re-render so the right ref populates

        if (Category is EquationSystemCategory.Linear && _linearInput is not null)
            _linearInput.SetValues(data.Coefficients ?? [], data.Rhs ?? []);
        else if (_equationList is not null && data.NonLinear is not null)
            await _equationList.SetFormDataAsync(data.NonLinear);
    }

    private async Task ExportPdfAsync()
    {
        if (Result is null) return;

        var methodName = Category is EquationSystemCategory.Linear
            ? $"Equation Systems — {LinearMethod}"
            : $"Equation Systems — {NonLinearMethod}";

        var inputs = new Dictionary<string, string>();
        
        if (Category is EquationSystemCategory.Linear && _linearInput is not null)
            inputs = _linearInput.GetLinearMethodInputs(Category, LinearMethod, Size);
        else if (_equationList is not null)
        {
            var formData = await _equationList.GetFormData();
            inputs = formData.GetNonLinearMethodInputs(Category, NonLinearMethod);
        }

        await ExportPdfCoreAsync(
            methodName: methodName,
            inputs: inputs,
            result: Result.GetResultSummary(),
            steps: Result.SolutionSteps,
            chartContainerId: IsChartVisible ? EquationSystemsUtils.ChartContainerId : null,
            fileName: $"equation-systems-{(Category is EquationSystemCategory.Linear ? LinearMethod : NonLinearMethod)}.pdf",
            type: CalculationType.EquationSystems);
    }
    
    private void ResetResult()
    {
        Result = null;
        LinearComparisonResult = null;
        NonLinearComparisonResult = null;
    }
}
