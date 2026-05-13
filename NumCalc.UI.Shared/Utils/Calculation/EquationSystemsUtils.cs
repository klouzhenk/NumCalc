using System.Text.Json;
using NumCalc.Shared.Enums.EquationSystems;
using NumCalc.Shared.EquationsSystems.Requests;
using NumCalc.Shared.EquationsSystems.Responses;
using NumCalc.UI.Shared.Components;
using NumCalc.UI.Shared.Components.EquationSystems;
using NumCalc.UI.Shared.Enums.Charts;
using NumCalc.UI.Shared.Enums.EquationSystems;
using NumCalc.UI.Shared.Models.Charts;
using NumCalc.UI.Shared.Models.EquationSystems;
using NumCalc.UI.Shared.Models.User;
using NumCalc.UI.Shared.Models.User.Enums;

namespace NumCalc.UI.Shared.Utils.Calculation;

public static class EquationSystemsUtils
{
    public const string ChartContainerId = "chart--equation-systems";
    
    public static NonLinearSystemRequest GetNonLinearCalculationRequest(this NonLinearSystemFormData formData)
    {
        return new NonLinearSystemRequest
        {
            IterationFunctions = formData.IterationFunctions.ToList(),
            Variables = formData.Variables.ToList(),
            InitialGuess = formData.InitialGuess.ToList(),
            Tolerance = formData.Tolerance,
            MaxIterations = formData.MaxIterations
        };
    }
    
    public static (SystemSolvingRequest request, List<string> variables, List<string> equations) GetLinearCalculationRequest(this LinearSystemInput linearInput, int size)
    {
        var variables = Enumerable.Range(1, size).Select(i => $"x{i}").ToList();
        var equations = BuildEquationStrings(linearInput.Coefficients, linearInput.Rhs, variables);
        var request = new SystemSolvingRequest
        {
            Equations = equations,
            Variables = variables
        };

        return (request, variables, equations);
    }
    
    public static SaveCalculationRecordRequest GetLinearHistoryRecord(
        this SystemSolvingResponse result,
        List<string> variables,
        List<string> equations,
        LinearSystemMethod method)
    {
        var inputs = new Dictionary<string, string> { ["Method"] = method.ToString() };
        for (var i = 0; i < equations.Count; i++)
            inputs[$"Equation {i + 1}"] = equations[i];
        inputs["Variables"] = string.Join(", ", variables);

        return new SaveCalculationRecordRequest
        {
            Type = CalculationType.EquationSystems,
            MethodName = method.ToString(),
            InputsJson = JsonSerializer.Serialize(inputs),
            ResultSummary = result.GetResultSummary(),
            ExecutionTimeMs = result.ExecutionTimeMs
        };
    }
    
    public static SaveCalculationRecordRequest GetNonLinearHistoryRecord(
        this SystemSolvingResponse result,
        NonLinearSystemFormData formData,
        NonLinearSystemMethod method)
    {
        var inputs = new Dictionary<string, string> { ["Method"] = method.ToString() };
        for (var i = 0; i < formData.IterationFunctions.Length; i++)
            inputs[$"Iteration Function {i + 1}"] = formData.IterationFunctions.ElementAt(i);
        inputs["Variables"] = string.Join(", ", formData.Variables);
        inputs["Initial Guess"] = string.Join(", ", formData.InitialGuess);
        inputs["Tolerance"] = formData.Tolerance.ToString("G");

        return new SaveCalculationRecordRequest
        {
            Type = CalculationType.EquationSystems,
            MethodName = method.ToString(),
            InputsJson = JsonSerializer.Serialize(inputs),
            ResultSummary = result.GetResultSummary(),
            ExecutionTimeMs = result.ExecutionTimeMs
        };
    }

    public static Dictionary<string, string> GetLinearMethodInputs(
        this LinearSystemInput linearInput,
        EquationSystemCategory category,
        LinearSystemMethod method,
        int size)
    {
        var inputs = new Dictionary<string, string>
        {
            ["Category"] = category.ToString(),
            ["Method"] = method.ToString()
        };
        
        var variables = Enumerable.Range(1, size).Select(i => $"x{i}").ToList();
        var equations = BuildEquationStrings(linearInput.Coefficients, linearInput.Rhs, variables);
        for (var i = 0; i < equations.Count; i++)
            inputs[$"Equation {i + 1}"] = equations[i];
        inputs["Variables"] = string.Join(", ", variables);
        
        return inputs;
    }
    
    public static Dictionary<string, string> GetNonLinearMethodInputs(
        this NonLinearSystemFormData formData,
        EquationSystemCategory category,
        NonLinearSystemMethod method)
    {
        var inputs = new Dictionary<string, string>
        {
            ["Category"] = category.ToString(),
            ["Method"] = method.ToString()
        };
        
        for (var i = 0; i < formData.IterationFunctions.Length; i++)
            inputs[$"Iteration Function {i + 1}"] = formData.IterationFunctions[i];
        inputs["Variables"] = string.Join(", ", formData.Variables);
        
        return inputs;
    }

    public static LinearSystemComparisonRequest GetLinearComparisonRequest(
        this LinearSystemInput linearInput,
        int size,
        List<LinearSystemMethod> linearBenchmarkMethods)
    {
        var variables = Enumerable.Range(1, size).Select(i => $"x{i}").ToList();
        var equations = BuildEquationStrings(linearInput.Coefficients, linearInput.Rhs, variables);

        return new LinearSystemComparisonRequest
        {
            Equations = equations,
            Variables = variables,
            Methods = linearBenchmarkMethods
        };
    }
    
    public static NonLinearSystemComparisonRequest GetNonLinearComparisonRequest(
        this NonLinearSystemFormData formData,
        List<NonLinearSystemMethod> nonLinearBenchmarkMethods)
    {
        return new NonLinearSystemComparisonRequest
        {
            IterationFunctions = formData.IterationFunctions.ToList(),
            Variables = formData.Variables.ToList(),
            InitialGuess = formData.InitialGuess.ToList(),
            Tolerance = formData.Tolerance,
            MaxIterations = formData.MaxIterations,
            Methods = nonLinearBenchmarkMethods
        };
    }

    public static Chart? Create2DChartConfig(
        this SystemSolvingResponse result, 
        NonLinearSystemFormData? nonLinearFormData,
        EquationSystemCategory category,
        int size)
    {
        var decimals = MathUtils.DecimalsFromTolerance(nonLinearFormData?.Tolerance);
        var xNames = GetXNames(nonLinearFormData, category, size);
        
        var series = result.ChartSeries?
            .Select((s, idx) => new ChartSeries
            {
                Name = s.Label,
                Data = s.Points
                    .Where(p => p is { X: not null, Y: not null })
                    .Select(p => new[] { p.X!.Value, p.Y!.Value })
                    .ToList(),
                Color = ColorUtils.GetSeriesColor(idx),
                LineWidth = 2,
                IsVisible = true
            })
            .ToList();
        
        if (series is not { Count: > 0 }) return null;

        if (result.Roots is { Count: >= 2 })
        {
            series.Add(new ChartSeries
            {
                Name = "Solution",
                Data = [[result.Roots[0], result.Roots[1]]],
                Type = ChartType.Scatter,
                Color = ColorUtils.GetColor(Enums.Color.PrimaryDark),
                LineWidth = 0,
                ZIndex = 5,
                IsVisible = true,
                Marker = new ChartMarker { Radius = 8, Symbol = ChartSymbolType.Diamond }
            });
        }

        return new Chart
        {
            ContainerId = ChartContainerId,
            ShowLegend = true,
            Decimals = decimals,
            XAxis = new ChartAxis { Title = xNames[0], PlotLines = [ChartUtils.CreateZeroLine()] },
            YAxis = new ChartAxis { Title = xNames[1], PlotLines = [ChartUtils.CreateZeroLine()] },
            Series = series
        };
    }
    
    public static Chart? Create3DChartConfig(
        this SystemSolvingResponse result, 
        NonLinearSystemFormData? nonLinearFormData,
        EquationSystemCategory category,
        int size)
    {
        var xNames = GetXNames(nonLinearFormData, category, size);
        var decimals = MathUtils.DecimalsFromTolerance(nonLinearFormData?.Tolerance);

        var series3d = result.ChartSeries?
            .Select((s, idx) => new ChartSeries
            {
                Name = s.Label,
                Data = s.Points
                    .Where(p => p is { X: not null, Y: not null, Z: not null })
                    .Select(p => new[] { p.X!.Value, p.Y!.Value, p.Z!.Value })
                    .ToList(),
                Color = ColorUtils.GetSeriesColor(idx),
                IsVisible = true
            })
            .ToList();

        if (series3d is not { Count: > 0 }) return null;

        if (result.Roots is { Count: >= 3 })
        {
            series3d.Add(new ChartSeries
            {
                Name = "Solution",
                Data = [[result.Roots[0], result.Roots[1], result.Roots[2]]],
                Type = ChartType.Scatter,
                Color = ColorUtils.GetColor(Enums.Color.PrimaryDark),
                IsVisible = true,
                Marker = new ChartMarker { Radius = 8, Symbol = ChartSymbolType.Diamond }
            });
        }

        return new Chart
        {
            ContainerId = ChartContainerId,
            ShowLegend = true,
            Decimals = decimals,
            XAxis = new ChartAxis { Title = xNames[0] },
            YAxis = new ChartAxis { Title = xNames[1] },
            ZAxis = new ChartAxis { Title = xNames[2] },
            Series = series3d
        };
    }

    public static string GetResultSummary(this SystemSolvingResponse result)
    {
         return result.Roots is { Count: > 0 }
            ? string.Join(",  ", result.Roots.Select((r, i) => $"x{i + 1} = {r.FormatResult()}"))
            : "No solution found";
    }
    
    public static string GetLinearInputSaveData(this LinearSystemInput linearInput)
    {
        var size = linearInput.Coefficients.GetLength(0);
        var rows = Enumerable.Range(0, size)
            .Select(i => Enumerable.Range(0, size).Select(j => linearInput.Coefficients[i, j]).ToArray())
            .ToArray();
        
        return JsonSerializer.Serialize(new
        {
            Category = nameof(EquationSystemCategory.Linear),
            Size = size,
            Coefficients = rows,
            Rhs = linearInput.Rhs
        });
    }
    
    public static async Task<string> GetNonLinearInputSaveData(this EquationList equationList)
    {
        var data = await equationList.GetFormData();
        return JsonSerializer.Serialize(new
        {
            Category = nameof(EquationSystemCategory.NonLinear),
            Size = data.IterationFunctions.Length,
            NonLinear = data
        });
    }
    
    private static List<string> GetXNames(
        NonLinearSystemFormData? nonLinearFormData,
        EquationSystemCategory category,
        int size)
    {
        var currentVariables = category is EquationSystemCategory.Linear
            ? Enumerable.Range(1, size).Select(i => $"x{i}").ToList()
            : nonLinearFormData?.Variables.ToList() ?? [];
        
        var x1Name = currentVariables.ElementAtOrDefault(0) ?? "x\u2081";
        var x2Name = currentVariables.ElementAtOrDefault(1) ?? "x\u2082";
        var x3Name = currentVariables.ElementAtOrDefault(2) ?? "x\u2083";

        return [x1Name, x2Name, x3Name];
    }
    
    private static List<string> BuildEquationStrings(double[,] coefficients, double[] rhs, List<string> variables)
    {
        var size = variables.Count;
        var equations = new List<string>(size);

        for (var row = 0; row < size; row++)
        {
            var terms = Enumerable.Range(0, size)
                .Select(col => $"{coefficients[row, col]}*{variables[col]}");
            equations.Add($"{string.Join(" + ", terms)} = {rhs[row]}");
        }

        return equations;
    }

    #region Saved input loading helpers

    public static (EquationSystemCategory category, int size) ParseCategoryAndSize(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var category = Enum.Parse<EquationSystemCategory>(root.GetProperty("Category").GetString()!);
        var size = root.GetProperty("Size").GetInt32();
        return (category, size);
    }

    public static void LoadFromJson(this LinearSystemInput input, string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var coefficients = root.GetProperty(nameof(LinearSystemInput.Coefficients)).Deserialize<double[][]>() ?? [];
        var rhs = root.GetProperty(nameof(LinearSystemInput.Rhs)).Deserialize<double[]>() ?? [];
        input.SetValues(coefficients, rhs);
    }

    public static async Task LoadFromJsonAsync(this EquationList list, string json)
    {
        using var doc = JsonDocument.Parse(json);
        var data = doc.RootElement.GetProperty(nameof(EquationSystemCategory.NonLinear))
            .Deserialize<NonLinearSystemFormData>();
        if (data is not null)
            await list.SetFormDataAsync(data);
    }

    #endregion
}
