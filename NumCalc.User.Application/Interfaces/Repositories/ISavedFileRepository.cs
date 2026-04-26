using NumCalc.User.Domain.Entities;

namespace NumCalc.User.Application.Interfaces.Repositories;

public interface ISavedFileRepository
{
    Task<List<SavedFile>> GetFilesMetadataByUserIdAsync(Guid userId);
    Task<List<SavedFile>> GetLastMetadataByUserIdAsync(Guid userId, int count);
    Task<SavedFile?> GetByIdAsync(Guid id);
    Task<int> CountByUserIdAsync(Guid userId);
    Task AddAsync(SavedFile file);
    Task DeleteAsync(Guid id);
    Task DeleteOldestByUserIdAsync(Guid userId);
    Task SaveChangesAsync();
}
