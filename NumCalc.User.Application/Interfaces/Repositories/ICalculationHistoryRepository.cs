using NumCalc.User.Domain.Entities;

namespace NumCalc.User.Application.Interfaces.Repositories;

public interface ICalculationHistoryRepository
{
    Task<CalculationHistoryRecord?> GetByIdAsync(Guid id);
    Task<List<CalculationHistoryRecord>> GetByUserIdAsync(Guid userId);
    Task<List<CalculationHistoryRecord>> GetLastByUserIdAsync(Guid userId, int count);
    Task<int> CountByUserIdAsync(Guid userId);
    Task AddAsync(CalculationHistoryRecord record);
    Task DeleteAsync(Guid id);
    Task DeleteOldestByUserIdAsync(Guid userId);
    Task SaveChangesAsync();
}
