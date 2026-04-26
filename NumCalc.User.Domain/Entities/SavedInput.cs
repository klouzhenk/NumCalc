using NumCalc.Shared.User.Enums;

namespace NumCalc.User.Domain.Entities;

public class SavedInput : BaseEntity
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public CalculationType Type { get; set; }
    public string InputsJson { get; set; } = string.Empty;
}
