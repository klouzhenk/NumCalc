using NumCalc.User.Domain.Enums;

namespace NumCalc.User.Application.DTOs;

public class SavePdfRequest
{
    public string FileName { get; set; } = string.Empty;
    public byte[] FileData { get; set; } = [];
    public CalculationType Type { get; set; }
    public string MethodName { get; set; } = string.Empty;
}
