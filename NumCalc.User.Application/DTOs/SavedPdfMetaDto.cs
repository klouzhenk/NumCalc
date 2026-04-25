using NumCalc.User.Domain.Enums;

namespace NumCalc.User.Application.DTOs;

public class SavedPdfMetaDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public CalculationType Type { get; set; }
    public string MethodName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
