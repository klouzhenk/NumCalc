using NumCalc.User.Domain.Entities;

namespace NumCalc.User.Application.Interfaces.Repositories;

public interface ISavedPdfExportRepository
{
    Task<List<SavedPdfExport>> GetMetaByUserIdAsync(Guid userId);
    Task<SavedPdfExport?> GetByIdAsync(Guid id);
    Task<int> CountByUserIdAsync(Guid userId);
    Task AddAsync(SavedPdfExport pdf);
    Task DeleteAsync(Guid id);
    Task SaveChangesAsync();
}
