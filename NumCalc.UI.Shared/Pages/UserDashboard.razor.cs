using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.User;

namespace NumCalc.UI.Shared.Pages;

public partial class UserDashboard : BasePage<UserDashboard>
{
    [Inject] private ICalculationHistoryApiService HistoryApiService { get; set; } = null!;
    [Inject] private ISavedInputApiService SavedInputApiService { get; set; } = null!;
    [Inject] private ISavedFileApiService SavedFileApiService { get; set; } = null!;

    private List<CalculationHistoryDto>? LastCalculationHistoryRecords { get; set; }
    private List<SavedInputDto>? LastSavedInputsData { get; set; }
    private List<SavedFileMetadataDto>? LastDownloadedFiles { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender && IsAuthenticated)
            await LoadUserDataAsync();
    }

    protected override async void OnAuthStateChanged()
    {
        if (IsAuthenticated)
        {
            await LoadUserDataAsync();
            return;
        }

        LastCalculationHistoryRecords = null;
        LastSavedInputsData = null;
        LastDownloadedFiles = null;
        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadUserDataAsync()
    {
        try
        {
            var historyTask = HistoryApiService.GetLastAsync(5);
            var inputsTask = SavedInputApiService.GetLastAsync(5);
            var filesTask = SavedFileApiService.GetLastAsync(5);

            await Task.WhenAll(historyTask, inputsTask, filesTask);

            LastCalculationHistoryRecords = historyTask.Result;
            LastSavedInputsData = inputsTask.Result;
            LastDownloadedFiles = filesTask.Result;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to load user data on main page");
        }

        await InvokeAsync(StateHasChanged);
    }
}