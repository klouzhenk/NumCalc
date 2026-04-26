using NumCalc.Shared.User.Enums;

namespace NumCalc.Shared.User.Requests;

public class SaveFileRequest
{
    public string FileName { get; set; } = string.Empty;
    public byte[] FileData { get; set; } = [];
    public CalculationType Type { get; set; }
    public string MethodName { get; set; } = string.Empty;
}
