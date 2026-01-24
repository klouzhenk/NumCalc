using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace NumCalc.UI.Shared.Components;

public partial class MathInput : ComponentBase
{
    [Parameter] public string? Label { get; set; }
    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string> ValueChanged { get; set; }
    [Parameter] public bool IsInvalid { get; set; }
    [Parameter] public string? ErrorMessage { get; set; }
    private ElementReference _mathFieldRef;
    private DotNetObjectReference<MathInput>? _dotNetRef;
    private bool _isInitialized;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            
            // Ініціалізуємо компонент через наш глобальний хелпер
            await JS.InvokeVoidAsync("NumCalc.initMathField", _mathFieldRef, _dotNetRef);
            
            // Встановлюємо початкове значення
            if (!string.IsNullOrEmpty(Value))
            {
                await JS.InvokeVoidAsync("NumCalc.setMathFieldValue", _mathFieldRef, Value);
            }

            _isInitialized = true;
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        // Якщо компонент вже ініціалізований і значення змінилося ззовні (не юзером),
        // оновлюємо його в полі
        if (_isInitialized)
        {
            await JS.InvokeVoidAsync("NumCalc.setMathFieldValue", _mathFieldRef, Value);
        }
    }

    [JSInvokable]
    public async Task UpdateValue(string newValue)
    {
        // Цей метод викликається з JS, коли юзер друкує
        if (Value != newValue)
        {
            Value = newValue;
            await ValueChanged.InvokeAsync(newValue);
        }
    }

    public async ValueTask DisposeAsync()
    {
        _dotNetRef?.Dispose();
    }
}