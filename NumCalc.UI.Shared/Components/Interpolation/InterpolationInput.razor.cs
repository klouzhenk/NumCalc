using Microsoft.AspNetCore.Components;
using NumCalc.Shared.Enums.Interpolation;
using NumCalc.UI.Shared.Models.Interpolation;

namespace NumCalc.UI.Shared.Components.Interpolation;

public partial class InterpolationInput : ComponentBase
{
    [Parameter] public InterpolationInputMode Mode { get; set; }

    private MathInput? _mathInput;
    private NodeTable? _nodeTable;

    public async Task<InterpolationFormData> GetFormData()
    {
        var formData = new InterpolationFormData
        {
            Mode = Mode,
            XNodes = _nodeTable?.GetXNodes() ?? [],
        };

        if (Mode is InterpolationInputMode.Function)
            formData.FunctionExpression = _mathInput is not null
                ? await _mathInput.GetAsciiValue()
                : string.Empty;
        else
            formData.YValues = _nodeTable?.GetYValues() ?? [];

        return formData;
    }
}
