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
    [Inject] private IOcrService OcrService { get; set; } = null!;
    [Inject] private IUiStateService UiStateService { get; set; } = null!;
    [Inject] private IStringLocalizer<Localization> Localizer { get; set; } = null!;
    
    private bool _isImageUploading;
    private BaseModal? _uploadImageModal;
    
    private async Task UploadImage(IBrowserFile file)
    {
        try
        {
            UiStateService.ShowLoader();
            var latexText = await OcrService.RecognizeExpression(file);
            if (string.IsNullOrWhiteSpace(latexText))
            {
                UiStateService.ShowError(Localizer["TEXT_NOT_RECOGNIZED"]);
                return;
            }
            
            if (OnInputContentRecognize.HasDelegate)
                await OnInputContentRecognize.InvokeAsync(latexText);

            await CloseImageUploadModal();
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

    private async Task ShowImageUploadModal()
    {
        if (_uploadImageModal is null) return;
        await _uploadImageModal.Show();
    }
    
    private async Task CloseImageUploadModal()
    {
        if (_uploadImageModal is null) return;
        await _uploadImageModal.Close();
    }
}