using Microsoft.AspNetCore.Components;
using NumCalc.Shared.Enums.ODE;
using NumCalc.UI.Shared.Models.ODE;

namespace NumCalc.UI.Shared.Components.ODE;

public partial class OdeInput : ComponentBase
{
    [Parameter] public OdeMethod Method { get; set; }

    private MathInput? _mathInput;

    private double _initialX;
    private double _initialY;
    private double _targetX = 1;
    private double _stepSize = 0.1;
    private int _picardOrder = 4;

    private int ComputedStepCount =>
        _stepSize > 0 ? (int)(Math.Abs(_targetX - _initialX) / _stepSize) : 0;

    public async Task<OdeFormData> GetFormData()
    {
        var expression = _mathInput is not null
            ? await _mathInput.GetAsciiValue()
            : string.Empty;

        return new OdeFormData
        {
            FunctionExpression = expression,
            InitialX = _initialX,
            InitialY = _initialY,
            TargetX = _targetX,
            StepSize = _stepSize,
            PicardOrder = Method is OdeMethod.Picard ? _picardOrder : null
        };
    }

    public async Task SetFormDataAsync(OdeFormData data)
    {
        _initialX = data.InitialX;
        _initialY = data.InitialY;
        _targetX = data.TargetX;
        _stepSize = data.StepSize;
        if (data.PicardOrder.HasValue) _picardOrder = data.PicardOrder.Value;
        StateHasChanged();
        if (!string.IsNullOrEmpty(data.FunctionExpression))
            await (_mathInput?.SetLatexValue(data.FunctionExpression) ?? Task.CompletedTask);
    }
}
