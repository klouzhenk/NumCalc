using Microsoft.AspNetCore.Components;

namespace NumCalc.UI.Shared.Components;

public partial class ComparisonResultList<TItem>
{
    [Parameter] public IEnumerable<TItem>? Items { get; set; }
    [Parameter] public Func<TItem, bool> IsBest { get; set; } = _ => false;
    [Parameter, EditorRequired] public RenderFragment<TItem> ItemTemplate { get; set; } = null!;
}