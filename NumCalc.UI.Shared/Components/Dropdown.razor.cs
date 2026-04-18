using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using NumCalc.UI.Shared.Resources;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.Components;

public partial class Dropdown<TItem> : ComponentBase, IDisposable
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
    [Inject] private IUiStateService UiStateService { get; set; } = null!;

    private readonly string _dropdownId = $"dropdown_{Guid.NewGuid()}";
    private bool IsAllSelected => Data.Any() && SelectedItems.Count == Data.Count();

    private async Task OnSelectAllChanged(ChangeEventArgs e)
    {
        if (e.Value is true) SelectedItems = Data.ToList();
        else SelectedItems.Clear();

        await SelectedItemsChanged.InvokeAsync(SelectedItems);
        StateHasChanged();
    }
    
    private bool _isOpen;

    private void ToggleDropdown() => _isOpen = !_isOpen;

    protected override void OnInitialized()
    {
        UiStateService.OnCloseDropdownRequested += Close;
    }

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
        StateHasChanged();
    }

    private void Close()
    {
        if (!_isOpen) return;
        _isOpen = false;
        InvokeAsync(StateHasChanged);
    }
    
    public void Dispose()
    {
        UiStateService.OnCloseDropdownRequested -= Close;
    }
}