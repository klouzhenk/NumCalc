using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using NumCalc.Shared.Enums.Interpolation;
using NumCalc.UI.Shared.Models.Interpolation;
using NumCalc.UI.Shared.Resources;

namespace NumCalc.UI.Shared.Components.Interpolation;

public partial class InterpolationInput : ComponentBase
{
    [Inject] private IStringLocalizer<Localization> Localizer { get; set; } = null!;

    private MathInput? _mathInput;
    private NodeTable? _nodeTable;
    private double _queryPoint;
    private InterpolationInputMode _mode = InterpolationInputMode.Function;

    public async Task<InterpolationFormData> GetFormData()
    {
        var formData = new InterpolationFormData
        {
            Mode = _mode,
            XNodes = _nodeTable?.GetXNodes() ?? [],
            QueryPoint = _queryPoint
        };

        if (_mode is InterpolationInputMode.Function)
            formData.FunctionExpression = _mathInput is not null
                ? await _mathInput.GetAsciiValue()
                : string.Empty;
        else
            formData.YValues = _nodeTable?.GetYValues() ?? [];

        return formData;
    }

    public async Task SetFormDataAsync(InterpolationFormData data)
    {
        _mode = data.Mode;
        _queryPoint = data.QueryPoint;
        _nodeTable?.SetValues(data.XNodes, data.YValues);
        StateHasChanged();
        if (!string.IsNullOrEmpty(data.FunctionExpression))
            await (_mathInput?.SetLatexValue(data.FunctionExpression) ?? Task.CompletedTask);
    }
}
