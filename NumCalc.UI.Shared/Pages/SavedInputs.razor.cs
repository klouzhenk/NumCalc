using System.Text.Json;
using Microsoft.AspNetCore.Components;
using NumCalc.UI.Shared.Components;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.User;

namespace NumCalc.UI.Shared.Pages;

public partial class SavedInputs : AuthorizedPage<SavedInputs>
{
    [Inject] private ISavedInputApiService SavedInputApiService { get; set; } = null!;

    private List<SavedInputDto>? SavedInputsData { get; set; }

    private BaseModal? _previewModal;
    private SavedInputDto? _previewItem;
    private IReadOnlyList<(string Key, string Value)> _previewFields = [];

    protected override Task OnAuthenticatedInitAsync() => LoadAsync();

    private async Task LoadAsync()
    {
        SavedInputsData = await SafeExecuteAsync(() => SavedInputApiService.GetSavedInputsAsync());
    }

    private async Task DeleteAsync(Guid id)
    {
        await SafeExecuteAsync(async () =>
        {
            await SavedInputApiService.DeleteSavedInputAsync(id);
            await LoadAsync();
        });
    }

    private void OpenPreview(SavedInputDto item)
    {
        _previewItem = item;
        _previewFields = ParseInputJson(item.InputsJson);
        _previewModal?.Show();
    }

    private static IReadOnlyList<(string Key, string Value)> ParseInputJson(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement
                .EnumerateObject()
                .Select(p => (p.Name, FormatValue(p.Value)))
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    private static string FormatValue(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Array => string.Join(", ", element.EnumerateArray().Select(e => e.ToString())),
        JsonValueKind.Null => "—",
        _ => element.ToString()
    };
}
