using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using NumCalc.UI.Shared.Resources;

namespace NumCalc.UI.Shared.Components;

public partial class CustomInputFile : ComponentBase
{
    [Parameter] public string? CssClass { get; set; }
    [Parameter] public bool CanTakePhoto { get; set; }
    [Parameter] public string Text { get; set; } = "UploadFile";
    [Parameter] public string? IconName { get; set; }
    [Parameter, EditorRequired] public EventCallback<IBrowserFile> OnFileUpload { get; set; }
    [Inject] private IStringLocalizer<Localization> Localizer { get; set; } = null!;
    
    private readonly Guid _inputId = Guid.NewGuid();

    private string InputId => $"input-file__{_inputId}";

    private void OnFileUploaded(InputFileChangeEventArgs e)
    {
        OnFileUpload.InvokeAsync(e.File);
    }
}