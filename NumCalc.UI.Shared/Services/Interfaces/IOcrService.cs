using Microsoft.AspNetCore.Components.Forms;

namespace NumCalc.UI.Shared.Services.Interfaces;

public interface IOcrService
{
    public Task<string> RecognizeExpression(IBrowserFile file);
    public Task<string> RecognizeExpression(string? imageBase64);
}