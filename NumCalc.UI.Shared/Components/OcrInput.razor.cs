using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using NumCalc.UI.Shared.Resources;
using NumCalc.UI.Shared.Services.Implementations;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.Components;

public partial class OcrInput : ComponentBase
{
    [Parameter] public EventCallback<string> OnInputContentRecognize { get; set; }
    [Inject] private OcrService OcrService { get; set; } = null!;
    [Inject] private IUiStateService UiStateService { get; set; } = null!;
    [Inject] private IStringLocalizer<Localization> Localizer { get; set; } = null!;
    
    private async Task UploadImage(InputFileChangeEventArgs e)
    {
        try
        {
            UiStateService.ShowLoader();
            var latexText = await OcrService.RecognizeExpression(e.File);
            if (string.IsNullOrWhiteSpace(latexText))
            {
                UiStateService.ShowError(Localizer["TEXT_NOT_RECOGNIZED"]);
                return;
            }
            
            if(OnInputContentRecognize.HasDelegate)
                await OnInputContentRecognize.InvokeAsync(latexText);
        }
        catch (Exception ex)
        {
            UiStateService.ShowError(Localizer[ex.Message]);
        }
        finally
        {
            UiStateService.HideLoader();
        }
    }
}