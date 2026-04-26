using NumCalc.Shared.User.Enums;

namespace NumCalc.Shared.User.DTOs;

public class SavedFileMetadataDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public CalculationType Type { get; set; }
    public string MethodName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
