using Microsoft.AspNetCore.Components;
using NumCalc.UI.Shared.Models.User.Enums;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.Components;

public partial class SavedInputDataPanel : ComponentBase
{
    [Inject] private IAuthStateService AuthStateService { get; set; } = null!;

    [Parameter] public CalculationType Type { get; set; }
    [Parameter] public EventCallback<string> OnSave { get; set; }
    [Parameter] public EventCallback<string> OnLoad { get; set; }

    private SavedInputDataPickerModal? _picker;
    private bool _showSaveForm;
    private string _saveInputName = string.Empty;

    private Task OpenPickerAsync() => _picker?.ShowAsync() ?? Task.CompletedTask;

    private async Task ConfirmAsync()
    {
        if (string.IsNullOrWhiteSpace(_saveInputName)) return;
        await OnSave.InvokeAsync(_saveInputName.Trim());
        _saveInputName = string.Empty;
        _showSaveForm = false;
    }
}
