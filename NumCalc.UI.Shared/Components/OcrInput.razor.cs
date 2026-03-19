using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using Microsoft.VisualBasic;
using NumCalc.UI.Shared.Components.Modals;
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
    
    private BaseModal? _uploadImageModal;
    private CropImageModal? _cropImageModal;
    private string? _uploadedImageBase64;
    
    private async Task UploadImage(IBrowserFile file)
    {
        try
        {
            UiStateService.ShowLoader();

            await using var stream = file.OpenReadStream(maxAllowedSize: 10485760);
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var buffer = memoryStream.ToArray();
            
            _uploadedImageBase64 = $"data:{file.ContentType};base64,{Convert.ToBase64String(buffer)}";
            ShowModal(ModalType.CropImage);
            await CloseModal(ModalType.UploadImage);
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

    private void ShowModal(ModalType type)
    {
        switch (type)
        {
            case ModalType.UploadImage:
                _uploadImageModal?.Show();
                break;
            case ModalType.CropImage:
                _cropImageModal?.Show();
                break;
            default:
                break;
        }
    }
    
    private async Task CloseModal(ModalType type)
    {
        switch (type)
        {
            case ModalType.UploadImage:
                if (_uploadImageModal is null) return;
                await _uploadImageModal.Close();
                break;
            case ModalType.CropImage:
                if (_cropImageModal is null) return;
                await _cropImageModal.Close();
                break;
            default:
                break;
        }
    }

    private void ShowImageUploadModal() => ShowModal(ModalType.UploadImage);
    
    private async Task HandleImageSubmit(string imageBase64)
    {
        try
        {
            UiStateService.ShowLoader();
            
            var latexText = await OcrService.RecognizeExpression(imageBase64);
            
            if (string.IsNullOrWhiteSpace(latexText))
            {
                UiStateService.ShowError(Localizer["TEXT_NOT_RECOGNIZED"]);
                return;
            }
                
            if (OnInputContentRecognize.HasDelegate)
                await OnInputContentRecognize.InvokeAsync(latexText);

            ClearImageData();
            await CloseAllOcrModals();
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

    private void ClearImageData()
    {
        _uploadedImageBase64 = null;
    }

    private async Task CloseAllOcrModals()
    {
        var closeUploadModal = CloseModal(ModalType.UploadImage);
        var closeCropModal = CloseModal(ModalType.CropImage);
        await Task.WhenAll(closeUploadModal, closeCropModal);
    }

    private enum ModalType
    {
        UploadImage,
        CropImage
    }
}