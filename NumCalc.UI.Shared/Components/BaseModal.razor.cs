using Microsoft.AspNetCore.Components;

namespace NumCalc.UI.Shared.Components;

public partial class BaseModal : ComponentBase
{
    [Parameter] public string? Title { get; set; }
    [Parameter] public string? Subtitle { get; set; }
    [Parameter] public RenderFragment? BodyContent { get; set; }
    [Parameter] public RenderFragment? FooterContent { get; set; }
    [Parameter] public bool CloseOnOutsideClick { get; set; } = true;
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public string? CssClass { get; set; }
    
    public bool IsVisible { get; private set; }
    
    public async Task Close()
    {
        IsVisible = false;
        StateHasChanged();
        
        if (OnClose.HasDelegate)
            await OnClose.InvokeAsync();
    }
    
    public void Show()
    {
        IsVisible = true;
        StateHasChanged();
    }

    private async Task HandleBackdropClick()
    {
        if (CloseOnOutsideClick)
            await Close();
    }
}