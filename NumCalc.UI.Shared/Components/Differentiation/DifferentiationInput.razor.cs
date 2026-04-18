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
    private NodeTable? _nodeTable;

    private double _queryPoint;
    private double _stepSize = 0.001;
    private int _derivativeOrder = 1;

    public async Task<DifferentiationFormData> GetFormData()
    {
        var formData = new DifferentiationFormData
        {
            QueryPoint = _queryPoint,
            StepSize = _stepSize,
            DerivativeOrder = _derivativeOrder,
            Mode = Mode,
            XNodes = _nodeTable?.GetXNodes() ?? []
        };

        if (Method is DifferentiationMethod.FiniteDifferences || Mode is DifferentiationInputMode.Function)
            formData.FunctionExpression = _mathInput is not null
                ? await _mathInput.GetAsciiValue()
                : string.Empty;

        if (Mode is DifferentiationInputMode.RawData)
            formData.YValues = _nodeTable?.GetYValues() ?? [];

        return formData;
    }
}
