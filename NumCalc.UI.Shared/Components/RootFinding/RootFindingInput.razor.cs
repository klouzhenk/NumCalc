using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using NumCalc.UI.Shared.Enums.Roots;
using NumCalc.UI.Shared.Models.RootFinding;
using NumCalc.UI.Shared.Resources;

namespace NumCalc.UI.Shared.Components.RootFinding;

public partial class RootFindingInput : ComponentBase
{
    [Parameter, EditorRequired] public required RootFindingFormData FormData { get; set; }
    [Parameter] public AnalysisMode Mode { get; set; }
    [Parameter] public EventCallback OnParametersChanged { get; set; }
    
    [Inject] protected IStringLocalizer<Localization> Localizer { get; set; } = null!;
    
    private MathInput? _mathInputComponent;
    
    private async Task ParametersChanged()
    {
        await OnParametersChanged.InvokeAsync();
    }

    public async Task<string?> GetAsciiExpressionAsync()
    {
        return _mathInputComponent is not null
            ? await _mathInputComponent.GetAsciiValue()
            : null;
    }

    public async Task SetLatexExpressionAsync(string? expression)
    {
        if (string.IsNullOrEmpty(expression)) return;
        
        await (_mathInputComponent?.SetLatexValue(expression) ?? Task.CompletedTask);
    }
}