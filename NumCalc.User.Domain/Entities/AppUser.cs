namespace NumCalc.User.Domain.Entities;

public class AppUser : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public ICollection<CalculationHistoryRecord> History { get; set; } = [];
    public ICollection<SavedInput> SavedInputs { get; set; } = [];
    public ICollection<SavedFile> SavedPdfs { get; set; } = [];
}
