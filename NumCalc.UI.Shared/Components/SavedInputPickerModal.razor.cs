using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.User;
using NumCalc.UI.Shared.Models.User.Enums;
using NumCalc.UI.Shared.Resources;

namespace NumCalc.UI.Shared.Components;

public partial class SavedInputPickerModal : ComponentBase
{
    [Inject] private ISavedInputApiService SavedInputApiService { get; set; } = null!;
    [Inject] private IStringLocalizer<Localization> Localizer { get; set; } = null!;

    [Parameter] public CalculationType Type { get; set; }
    [Parameter] public EventCallback<string> OnInputSelected { get; set; }

    private BaseModal? _modal;
    private List<SavedInputDto>? _items;

    public async Task ShowAsync()
    {
        _items = null;
        _modal?.Show();
        _items = await SavedInputApiService.GetByTypeAsync(Type);
        StateHasChanged();
    }

    private async Task SelectAsync(SavedInputDto item)
    {
        await _modal!.Close();
        await OnInputSelected.InvokeAsync(item.InputsJson);
    }
}
