using NumCalc.User.Application.DTOs;

namespace NumCalc.User.Application.Interfaces.Services;

public interface ICalculationHistoryService
{
    Task<List<CalculationHistoryDto>> GetCalculationHistoryAsync(Guid userId);
    Task<List<CalculationHistoryDto>> GetLastAsync(Guid userId, int count);
    Task SaveAsync(Guid userId, SaveCalculationRecordRequest request);
    Task DeleteAsync(Guid userId, Guid recordId);
}
