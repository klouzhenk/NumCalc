using NumCalc.User.Domain.Entities;

namespace NumCalc.User.Application.Interfaces.Repositories;

public interface ICalculationHistoryRepository
{
    Task<List<CalculationHistory>> GetByUserIdAsync(Guid userId);
    Task<int> CountByUserIdAsync(Guid userId);
    Task AddAsync(CalculationHistory history);
    Task DeleteAsync(Guid id);
    Task DeleteOldestByUserIdAsync(Guid userId);
    Task SaveChangesAsync();
}
