using NumCalc.Shared.User.Enums;

namespace NumCalc.Shared.User.DTOs;

public class SavedInputDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public CalculationType Type { get; set; }
    public string InputsJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
