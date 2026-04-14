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
}