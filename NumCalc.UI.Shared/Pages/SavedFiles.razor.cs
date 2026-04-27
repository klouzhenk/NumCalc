using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.User;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.Pages;

public partial class SavedFiles : BasePage<SavedFiles>
{
    [Inject] private ISavedFileApiService SavedFileApiService { get; set; } = null!;
    [Inject] private IAuthStateService AuthStateService { get; set; } = null!;

    private List<SavedFileMetadataDto>? Files { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (!AuthStateService.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
            return;
        }

        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        Files = await SafeExecuteAsync(() => SavedFileApiService.GetSavedFilesAsync());
    }

    private async Task DownloadAsync(Guid id, string fileName)
    {
        await SafeExecuteAsync(async () =>
        {
            var bytes = await SavedFileApiService.DownloadFileAsync(id);
            if (bytes is null) return;
            await JsRuntime.InvokeVoidAsync("PdfHelper.downloadFile", fileName, "application/pdf", bytes);
        });
    }

    private async Task DeleteAsync(Guid id)
    {
        await SafeExecuteAsync(async () =>
        {
            await SavedFileApiService.DeleteFileAsync(id);
            await LoadAsync();
        });
    }
}
