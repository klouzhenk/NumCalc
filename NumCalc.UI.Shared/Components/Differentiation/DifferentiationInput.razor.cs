using Microsoft.AspNetCore.Components;
using NumCalc.Shared.Enums.Differentiation;
using NumCalc.UI.Shared.Enums.Differentiation;
using NumCalc.UI.Shared.Models.Differentiation;

namespace NumCalc.UI.Shared.Components.Differentiation;

public partial class DifferentiationInput : ComponentBase
{
    [Parameter] public DifferentiationMethod Method { get; set; }
    [Parameter] public DifferentiationInputMode Mode { get; set; }

    private MathInput? _mathInput;

    // Finite differences fields
    private double _queryPoint;
    private double _stepSize = 0.001;
    private int _derivativeOrder = 1;

    // Lagrange fields
    private int _nodeCount = 4;
    private List<double> _xNodes = new(Enumerable.Repeat(0.0, 4));
    private List<double> _yValues = new(Enumerable.Repeat(0.0, 4));

    private void AddNode()
    {
        _nodeCount++;
        _xNodes.Add(0.0);
        _yValues.Add(0.0);
    }

    private void RemoveNode()
    {
        if (_nodeCount <= 2) return;
        _nodeCount--;
        _xNodes.RemoveAt(_nodeCount);
        _yValues.RemoveAt(_nodeCount);
    }

    public async Task<DifferentiationFormData> GetFormData()
    {
        var formData = new DifferentiationFormData
        {
            QueryPoint = _queryPoint,
            StepSize = _stepSize,
            DerivativeOrder = _derivativeOrder,
            Mode = Mode,
            XNodes = [.. _xNodes]
        };

        if (Method is DifferentiationMethod.FiniteDifferences || Mode is DifferentiationInputMode.Function)
            formData.FunctionExpression = _mathInput is not null
                ? await _mathInput.GetAsciiValue()
                : string.Empty;

        if (Mode is DifferentiationInputMode.RawData)
            formData.YValues = [.. _yValues];

        return formData;
    }
}
