using NumCalc.User.Domain.Entities;

namespace NumCalc.User.Application.Interfaces.Repositories;

public interface ICalculationHistoryRepository
{
    Task<List<CalculationHistoryRecord>> GetByUserIdAsync(Guid userId);
    Task<int> CountByUserIdAsync(Guid userId);
    Task AddAsync(CalculationHistoryRecord record);
    Task DeleteAsync(Guid id);
    Task DeleteOldestByUserIdAsync(Guid userId);
    Task SaveChangesAsync();
}
