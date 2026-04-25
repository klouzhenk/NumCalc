using NumCalc.User.Domain.Enums;

namespace NumCalc.User.Domain.Entities;

public class SavedFile : BaseEntity
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public string FileName { get; set; } = string.Empty;
    public byte[] FileData { get; set; } = [];
    public CalculationType Type { get; set; }
    public string MethodName { get; set; } = string.Empty;
}
