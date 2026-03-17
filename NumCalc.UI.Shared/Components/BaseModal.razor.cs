using Microsoft.AspNetCore.Components;

namespace NumCalc.UI.Shared.Components;

public partial class BaseModal : ComponentBase
{
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
    [Parameter] public string? Title { get; set; }
    [Parameter] public string? Subtitle { get; set; }
    [Parameter] public RenderFragment? BodyContent { get; set; }
    [Parameter] public RenderFragment? FooterContent { get; set; }
    [Parameter] public bool CloseOnOutsideClick { get; set; } = true;
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public string? CssClass { get; set; }
    
    public async Task Close()
    {
        IsVisible = false;
        await IsVisibleChanged.InvokeAsync(IsVisible);
        await OnClose.InvokeAsync();
    }
    
    public async Task Show()
    {
        IsVisible = true;
        await IsVisibleChanged.InvokeAsync(IsVisible);
    }

    private async Task HandleBackdropClick()
    {
        if (CloseOnOutsideClick)
            await Close();
    }
}