using NumCalc.User.Domain.Entities;

namespace NumCalc.User.Application.Interfaces.Repositories;

public interface ISavedInputRepository
{
    Task<List<SavedInput>> GetByUserIdAsync(Guid userId);
    Task<SavedInput?> GetByIdAsync(Guid id);
    Task AddAsync(SavedInput input);
    Task DeleteAsync(Guid id);
    Task SaveChangesAsync();
}
