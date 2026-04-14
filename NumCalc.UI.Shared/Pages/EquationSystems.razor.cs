using Microsoft.AspNetCore.Components;
using NumCalc.Shared.EquationsSystems.Requests;
using NumCalc.Shared.EquationsSystems.Responses;
using NumCalc.UI.Shared.Components;
using NumCalc.UI.Shared.Components.EquationSystems;
using NumCalc.UI.Shared.Enums.EquationSystems;
using NumCalc.UI.Shared.HttpServices.Interfaces;

namespace NumCalc.UI.Shared.Pages;

public partial class EquationSystems : BasePage<EquationSystems>
{
    [Inject] private ICalculationApiService CalculationApiService { get; set; } = null!;

    private EquationSystemCategory Category { get; set; } = EquationSystemCategory.Linear;
    private LinearSystemMethod LinearMethod { get; set; } = LinearSystemMethod.Cramer;
    private NonLinearSystemMethod NonLinearMethod { get; set; } = NonLinearSystemMethod.FixedPoint;
    private int Size { get; set; } = 2;

    private readonly int[] _sizes = [2, 3, 4];

    private LinearSystemInput? _linearInput;
    private EquationList? _equationList;

    private SystemSolvingResponse? Result { get; set; }

    private async Task Calculate()
    {
        Result = null;

        if (Category is EquationSystemCategory.Linear)
            await CalculateLinear();
        else
            await CalculateNonLinear();
    }

    private async Task CalculateLinear()
    {
        if (_linearInput is null) return;

        var variables = Enumerable.Range(1, Size).Select(i => $"x{i}").ToList();

        var request = new SystemSolvingRequest
        {
            Equations = BuildEquationStrings(_linearInput.Coefficients, _linearInput.Rhs, variables),
            Variables = variables
        };

        Func<Task<SystemSolvingResponse?>> apiCall = LinearMethod switch
        {
            LinearSystemMethod.Cramer => () => CalculationApiService.SolveCramerAsync(request),
            LinearSystemMethod.Gauss  => () => CalculationApiService.SolveGaussianAsync(request),
            _ => throw new ArgumentOutOfRangeException(nameof(LinearMethod))
        };

        Result = await SafeExecuteAsync(apiCall);
    }

    private async Task CalculateNonLinear()
    {
        if (_equationList is null) return;

        var formData = await _equationList.GetFormData();

        var request = new NonLinearSystemRequest
        {
            IterationFunctions = formData.IterationFunctions.ToList(),
            Variables = formData.Variables.ToList(),
            InitialGuess = formData.InitialGuess.ToList(),
            Tolerance = formData.Tolerance,
            MaxIterations = formData.MaxIterations
        };

        Func<Task<SystemSolvingResponse?>> apiCall = NonLinearMethod switch
        {
            NonLinearSystemMethod.FixedPoint => () => CalculationApiService.SolveFixedPointAsync(request),
            NonLinearSystemMethod.Seidel     => () => CalculationApiService.SolveSeidelAsync(request),
            _ => throw new ArgumentOutOfRangeException(nameof(NonLinearMethod))
        };

        Result = await SafeExecuteAsync(apiCall);
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
}
