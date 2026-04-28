using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using NumCalc.UI.Shared.Models.User.Enums;
using NumCalc.UI.Shared.Resources;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.Components;

public partial class SavedInputDataPanel : ComponentBase
{
    [Inject] private IAuthStateService AuthStateService { get; set; } = null!;
    [Inject] private IStringLocalizer<Localization> Localizer { get; set; } = null!;

    [Parameter] public CalculationType Type { get; set; }
    [Parameter] public EventCallback<string> OnSave { get; set; }
    [Parameter] public EventCallback<string> OnLoad { get; set; }

    private SavedInputDataSaveModal? _saveModal;
    private SavedInputDataPickerModal? _picker;

    private async Task OpenPicker()
    {
        if (_picker is null) return;
        await _picker.Show();
    }
    
    private void OpenSaveModal() => _saveModal?.Show();
}
