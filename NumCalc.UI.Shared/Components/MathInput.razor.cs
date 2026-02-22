using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using NumCalc.UI.Shared.Resources;
using NumCalc.UI.Shared.Services.Implementations;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.Components;

public partial class MathInput : ComponentBase, IDisposable
{
    [Parameter] public string? Label { get; set; }
    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string> ValueChanged { get; set; }
    [Parameter] public EventCallback<string> OnInput { get; set; }
    [Inject] public IJSRuntime JsRuntime { get; set; } = null!;
    [Inject] protected IUiStateService UiStateService { get; set; } = null!;
    [Inject] public IStringLocalizer<Localization> Localizer { get; set; } = null!;
    
    private ElementReference _mathFieldRef;
    private DotNetObjectReference<MathInput>? _dotNetRef;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            await JsRuntime.InvokeVoidAsync("NumCalc.initMathField", _mathFieldRef, _dotNetRef);
            
            if (!string.IsNullOrEmpty(Value)) {
                await JsRuntime.InvokeVoidAsync("NumCalc.setMathFieldValue", _mathFieldRef, Value);
            }
        }
    }
    
    [JSInvokable]
    public async Task UpdateValue(string newValue)
    {
        if (Value == newValue)
            return;

        Value = newValue;
        await ValueChanged.InvokeAsync(newValue);

        if (OnInput.HasDelegate)
            await OnInput.InvokeAsync(newValue);
    }

    public async Task<string> GetAsciiValue()
    {
        return await JsRuntime.InvokeAsync<string>("NumCalc.getAsciiFromMathField", _mathFieldRef);
    }
    
    public async Task SetLatexValue(string latex)
    {
        await JsRuntime.InvokeVoidAsync("NumCalc.setLatexInMathField", _mathFieldRef, latex);
    }

    private async Task CopyToClipboard()
    {
        if (!string.IsNullOrEmpty(Value))
        {
            await JsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", Value);
            UiStateService.ShowSuccess(Localizer["Copied"]); 
        }
    }

    public void Dispose()
    {
        _dotNetRef?.Dispose();
    }
}