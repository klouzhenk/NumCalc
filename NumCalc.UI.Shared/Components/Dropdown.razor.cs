using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using NumCalc.UI.Shared.Resources;

namespace NumCalc.UI.Shared.Components;

public partial class Dropdown<TItem> : ComponentBase
{
    [Parameter] public required string Label { get; set; }
    [Parameter] public IEnumerable<TItem> Data { get; set; } = [];
    [Parameter] public Func<TItem, string> TextField { get; set; } = x => x?.ToString() ?? "";
    [Parameter] public TItem? Value { get; set; }
    [Parameter] public EventCallback<TItem?> ValueChanged { get; set; }
    [Parameter] public bool MultiSelect { get; set; }
    [Parameter] public List<TItem> SelectedItems { get; set; } = [];
    [Parameter] public EventCallback<List<TItem>> SelectedItemsChanged { get; set; }

    [Inject] private IStringLocalizer<Localization> Localizer { get; set; } = null!;
    
    private bool IsAllSelected => Data.Any() && SelectedItems.Count == Data.Count();
    
    private bool _isOpen;

    private void ToggleDropdown() => _isOpen = !_isOpen;
    private void Close() => _isOpen = false;

    private bool IsSelected(TItem item)
    {
        return MultiSelect 
            ? SelectedItems.Contains(item) 
            : EqualityComparer<TItem>.Default.Equals(Value, item);
    }

    private string GetSelectedText()
    {
        if (!MultiSelect) 
            return Value != null ? TextField(Value) : Localizer["Select"];
        
        if (SelectedItems.Count == 0) 
            return Localizer["SelectItems"];
        
        if (SelectedItems.Count == Data.Count()) 
            return Localizer["AllSelected"];

        if (SelectedItems.Count != 1)
            return string.Join(", ",
                SelectedItems.Select(x => x is null ? "null" : Localizer[x.ToString() ?? string.Empty].Value));
        
        return TextField(SelectedItems.First());
    }

    private async Task OnItemClick(TItem item)
    {
        if (!MultiSelect)
        {
            Value = item;
            await ValueChanged.InvokeAsync(Value);
            Close();
            return;
        }
        
        if (!SelectedItems.Remove(item))
            SelectedItems.Add(item);

        await SelectedItemsChanged.InvokeAsync(SelectedItems);
    }

    private async Task ToggleSelectAll()
    {
        if (IsAllSelected)
        {
            SelectedItems.Clear();
        }
        else
        {
            SelectedItems.Clear();
            SelectedItems.AddRange(Data);
        }

        await SelectedItemsChanged.InvokeAsync(SelectedItems);
    }
}