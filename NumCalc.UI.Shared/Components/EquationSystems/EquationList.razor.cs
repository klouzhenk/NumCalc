using Microsoft.AspNetCore.Components;
using NumCalc.UI.Shared.Models.EquationSystems;

namespace NumCalc.UI.Shared.Components.EquationSystems;

public partial class EquationList : ComponentBase
{
    [Parameter] public int Size { get; set; }

    private MathInput[] MathInputs { get; set; } = [];
    private double[] InitialGuess { get; set; } = [];
    private double Tolerance { get; set; } = 1e-6;
    private int MaxIterations { get; set; } = 500;

    protected override void OnParametersSet()
    {
        if (MathInputs.Length == Size) return;

        MathInputs = new MathInput[Size];
        InitialGuess = new double[Size];
    }

    public async Task<NonLinearSystemFormData> GetFormData()
    {
        var functions = new string[Size];
        for (var i = 0; i < Size; i++)
            functions[i] = MathInputs[i] is not null
                ? await MathInputs[i].GetAsciiValue()
                : string.Empty;

        return new NonLinearSystemFormData
        {
            IterationFunctions = functions,
            Variables = Enumerable.Range(1, Size).Select(i => $"x{i}").ToArray(),
            InitialGuess = (double[])InitialGuess.Clone(),
            Tolerance = Tolerance,
            MaxIterations = MaxIterations
        };
    }
}
