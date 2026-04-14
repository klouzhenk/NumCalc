using Microsoft.AspNetCore.Components;
using NumCalc.Shared.Enums.Interpolation;
using NumCalc.UI.Shared.Models.Interpolation;

namespace NumCalc.UI.Shared.Components.Interpolation;

public partial class InterpolationInput : ComponentBase
{
    [Parameter] public InterpolationInputMode Mode { get; set; }

    private MathInput? _mathInput;
    private int _nodeCount = 4;
    private List<double> _xNodes = new(Enumerable.Repeat(0.0, 4));
    private List<double> _yValues = new(Enumerable.Repeat(0.0, 4));
    private double _queryPoint;

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

    public async Task<InterpolationFormData> GetFormData()
    {
        var formData = new InterpolationFormData
        {
            Mode = Mode,
            XNodes = [.. _xNodes],
            QueryPoint = _queryPoint
        };

        if (Mode is InterpolationInputMode.Function)
            formData.FunctionExpression = _mathInput is not null
                ? await _mathInput.GetAsciiValue()
                : string.Empty;
        else
            formData.YValues = [.. _yValues];

        return formData;
    }
}
