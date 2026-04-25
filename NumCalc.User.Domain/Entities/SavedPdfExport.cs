using NumCalc.User.Domain.Enums;

namespace NumCalc.User.Domain.Entities;

public class SavedPdfExport : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string FileName { get; set; } = string.Empty;
    public byte[] FileData { get; set; } = [];
    public CalculationDomain Domain { get; set; }
    public string MethodName { get; set; } = string.Empty;
}
