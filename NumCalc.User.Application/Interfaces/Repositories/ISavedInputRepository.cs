using NumCalc.User.Domain.Entities;
using NumCalc.User.Domain.Enums;

namespace NumCalc.User.Application.Interfaces.Repositories;

public interface ISavedInputRepository
{
    Task<List<SavedInput>> GetByUserIdAsync(Guid userId);
    Task<List<SavedInput>> GetByUserIdAndTypeAsync(Guid userId, CalculationType type);
    Task<SavedInput?> GetByIdAsync(Guid id);
    Task AddAsync(SavedInput input);
    Task DeleteAsync(Guid id);
    Task SaveChangesAsync();
}
