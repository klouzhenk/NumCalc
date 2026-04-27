using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NumCalc.Shared.Common;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.Export;
using NumCalc.UI.Shared.Models.User;
using NumCalc.UI.Shared.Models.User.Enums;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.Pages.Calculation;

public abstract class CalculationPage<TPage> : BasePage<TPage>
{
    [Inject] private IPdfExportService PdfExportService { get; set; } = null!;
    [Inject] private ICalculationHistoryApiService HistoryApiService { get; set; } = null!;
    [Inject] private ISavedFileApiService SavedFileApiService { get; set; } = null!;
    [Inject] private ISavedInputApiService SavedInputApiService { get; set; } = null!;
 
    private string? _lastSavedHash;
    
    protected async Task ExportPdfCoreAsync(
        string methodName,
        Dictionary<string, string> inputs,
        string result,
        IEnumerable<SolutionStep>? steps,
        string? chartContainerId,
        string fileName,
        CalculationType type)
    {
        await SafeExecuteAsync(async () =>
        {
            var stepItems = new List<StepExportItem>();
            foreach (var step in steps ?? [])
            {
                string? imageBase64 = null;
                if (!string.IsNullOrWhiteSpace(step.LatexFormula))
                    imageBase64 = await JsRuntime.InvokeAsync<string>("PdfHelper.renderLatexToPng", step.LatexFormula);
                stepItems.Add(new StepExportItem { Description = step.Description, ImageBase64 = imageBase64, Value = step.Value });
            }

            var chartImage = chartContainerId is not null
                ? await JsRuntime.InvokeAsync<string>("PdfHelper.getChartImage", chartContainerId)
                : null;

            var request = new SavedFileRequest
            {
                MethodName = methodName,
                Inputs = inputs,
                Result = result,
                Steps = stepItems,
                ChartImage = chartImage
            };

            var pdfBytes = PdfExportService.GeneratePdf(request);
            await TrySaveFileAsync(fileName, pdfBytes, type, methodName);
            var base64 = Convert.ToBase64String(pdfBytes);
            await JsRuntime.InvokeVoidAsync("PdfHelper.downloadFile", fileName, "application/pdf", base64);
        });
    }
    
    private async Task TrySaveFileAsync(string fileName, byte[] fileData, CalculationType type, string methodName)
    {
        if (!AuthStateService.IsAuthenticated) return;
        try
        {
            await SavedFileApiService.SaveFileAsync(new SaveFileRequest
            {
                FileName = fileName,
                FileData = fileData,
                Type = type,
                MethodName = methodName
            });
        }
        catch { /* silent — auto-save is best-effort */ }
    }

    protected async Task TrySaveInputAsync(string name, CalculationType type, string inputsJson)
    {
        if (!AuthStateService.IsAuthenticated) return;
        try
        {
            await SavedInputApiService.CreateSavedInputAsync(new CreateSavedInputRequest
            {
                Name = name,
                Type = type,
                InputsJson = inputsJson
            });
        }
        catch { /* silent — best-effort */ }
    }

    protected async Task TrySaveHistoryAsync(SaveCalculationRecordRequest record)
    {
        if (!AuthStateService.IsAuthenticated) return;

        var hash = $"{record.MethodName}|{record.InputsJson}";
        if (hash == _lastSavedHash) return;

        try
        {
            await HistoryApiService.SaveHistoryAsync(record);
            _lastSavedHash = hash;
        }
        catch { /* silent — auto-save is best-effort */ }
    }
}