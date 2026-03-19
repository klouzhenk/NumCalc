using Cropper.Blazor.Components;
using Cropper.Blazor.Exceptions;
using Cropper.Blazor.Extensions;
using Cropper.Blazor.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using NumCalc.UI.Shared.Resources;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.Components.Modals;

public partial class CropImageModal : ComponentBase
{
    [Parameter] public string ImageBase64 { get; set; } = string.Empty;
    [Parameter] public EventCallback<string> OnSubmit { get; set; }
    [Inject] private IUiStateService UiStateService { get; set; } = null!;
    [Inject] private IStringLocalizer<Localization> Localizer { get; set; } = null!;
    [Inject] private IJSRuntime JsRuntime { get; set; } = null!;

    public bool IsVisible => _modal?.IsVisible ?? false;
    
    private CropperComponent? _cropperComponent;
    private BaseModal? _modal;
    private readonly Options _cropperOptions = new()
    {
        AspectRatio = 0,
        ViewMode = ViewMode.Vm0,
        DragMode = DragMode.Crop.ToEnumString() ?? string.Empty,
        AutoCropArea = 1
    };
    
    private void RotateLeft()
    {
        _cropperComponent?.Rotate(-90);
    }
    
    private void RotateRight()
    {
        _cropperComponent?.Rotate(90);
    }

    private async Task ApplyAndRecognize()
    {
        try
        {
            var elementRef = _cropperComponent?.GetCropperElementReference();
            if (elementRef is null) return;
            
            var base64 = await JsRuntime.InvokeAsync<string>("ImageHelper.getCroppedImageBase64", elementRef.Value);
            if (string.IsNullOrEmpty(base64)) return;

            if (OnSubmit.HasDelegate)
                await OnSubmit.InvokeAsync(base64);

            await Close();
        }
        catch (Exception ex)
        {
            
        }
    }
    
    public void Show() => _modal?.Show();

    public async Task Close()
    {
        if (_modal is null) return;
        await _modal.Close();
    }
}