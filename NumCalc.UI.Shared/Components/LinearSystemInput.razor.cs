using Microsoft.AspNetCore.Components;

namespace NumCalc.UI.Shared.Components;

public partial class LinearSystemInput : ComponentBase
{
    [Parameter] public int Size { get; set; }

    public double[,] Coefficients { get; private set; }
    public double[] Rhs { get; private set; }
    
    protected override void OnParametersSet()
    {
        if (Coefficients?.GetLength(0) == Size) return;

        Coefficients = new double[Size, Size];
        Rhs = new double[Size];
    }

    public void SetValues(double[][] coefficients, double[] rhs)
    {
        var size = coefficients.Length;
        Coefficients = new double[size, size];
        for (var i = 0; i < size; i++)
            for (var j = 0; j < size; j++)
                Coefficients[i, j] = coefficients[i][j];
        Rhs = rhs;
        StateHasChanged();
    }
}