using NumCalc.UI.Shared.Models.User.Enums;

namespace NumCalc.UI.Shared.Models.User;

public class SaveFileRequest
{
    public string FileName { get; set; } = string.Empty;
    public byte[] FileData { get; set; } = [];
    public CalculationType Type { get; set; }
    public string MethodName { get; set; } = string.Empty;
}
