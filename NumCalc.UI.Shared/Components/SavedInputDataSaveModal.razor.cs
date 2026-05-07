using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using NumCalc.UI.Shared.Resources;

namespace NumCalc.UI.Shared.Components;

public partial class SavedInputDataSaveModal : ComponentBase
{
    [Inject] private IStringLocalizer<Localization> Localizer { get; set; } = null!;

    [Parameter] public EventCallback<string> OnSave { get; set; }

    private BaseModal? _modal;
    private string _name = string.Empty;

    public void Show()
    {
        _name = string.Empty;
        _modal?.Show();
    }

    private async Task ConfirmAsync()
    {
        if (string.IsNullOrWhiteSpace(_name)) return;
        await CloseModal();
        await OnSave.InvokeAsync(_name.Trim());
    }

    private async Task CloseModal()
    {
        if (_modal is null) return;
        await _modal.Close();
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter") 
            await ConfirmAsync();
    }
}
