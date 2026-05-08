using NumCalc.UI.Shared.Models.User.Enums;

namespace NumCalc.UI.Shared.Models.User;

public class SavedInputDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public CalculationType Type { get; set; }
    public string InputsJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
