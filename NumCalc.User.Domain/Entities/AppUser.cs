namespace NumCalc.User.Domain.Entities;

public class AppUser : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public ICollection<CalculationHistory> History { get; set; } = [];
    public ICollection<SavedInput> SavedInputs { get; set; } = [];
    public ICollection<SavedPdfExport> SavedPdfs { get; set; } = [];
}
