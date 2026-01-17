using Microsoft.AspNetCore.Components;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.Components;

public partial class GlobalLoader : ComponentBase, IDisposable
{
    [Inject] protected IUiStateService UiStateService { get; set; } = null!;
    
    private bool _isVisible;

    protected override void OnInitialized()
    {
        UiStateService.OnLoaderChanged += HandleLoaderState;
    }

    private void HandleLoaderState(bool show)
    {
        InvokeAsync(() =>
        {
            _isVisible = show;
            StateHasChanged();
        });
    }

    public void Dispose()
    {
        UiStateService.OnLoaderChanged -= HandleLoaderState;
    }
}