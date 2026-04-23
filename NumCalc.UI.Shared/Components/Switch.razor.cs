using Microsoft.AspNetCore.Components;

namespace NumCalc.UI.Shared.Components;

public partial class Switch<TItem> : ComponentBase
{
    [Parameter] public string? Label { get; set; }
    [Parameter] public IEnumerable<TItem> Data { get; set; } = [];
    
    [Parameter] public TItem? Value { get; set; }
    [Parameter] public EventCallback<TItem?> ValueChanged { get; set; }
    
    [Parameter] public Func<TItem, string> TextField { get; set; } = x => x?.ToString() ?? string.Empty;
    [Parameter] public bool Shrink { get; set; } = false;

    private bool IsSelected(TItem item)
    {
        return EqualityComparer<TItem>.Default.Equals(Value, item);
    }

    private async Task SelectItem(TItem item)
    {
        if (!IsSelected(item))
        {
            Value = item;
            await ValueChanged.InvokeAsync(Value);
        }
    }
}