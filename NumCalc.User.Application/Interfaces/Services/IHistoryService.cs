using NumCalc.User.Application.DTOs;

namespace NumCalc.User.Application.Interfaces.Services;

public interface IHistoryService
{
    Task<List<CalculationHistoryDto>> GetHistoryAsync(Guid userId);
    Task SaveAsync(Guid userId, SaveHistoryRequest request);
    Task DeleteAsync(Guid userId, Guid historyId);
}
