using Microsoft.AspNetCore.Components;
using NumCalc.UI.Shared.Models.Integration;

namespace NumCalc.UI.Shared.Components.Integration;

public partial class IntegrationInput : ComponentBase
{
    private MathInput? _mathInput;
    private double _lowerBound;
    private double _upperBound = 1;
    private int _intervals = 100;

    public async Task<IntegrationFormData> GetFormData()
    {
        return new IntegrationFormData
        {
            FunctionExpression = _mathInput is not null
                ? await _mathInput.GetAsciiValue()
                : string.Empty,
            LowerBound = _lowerBound,
            UpperBound = _upperBound,
            Intervals = _intervals
        };
    }
}
