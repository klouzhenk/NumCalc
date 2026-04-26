using NumCalc.Shared.User.DTOs;
using NumCalc.Shared.User.Requests;

namespace NumCalc.User.Application.Interfaces.Services;

public interface ICalculationHistoryService
{
    Task<List<CalculationHistoryDto>> GetCalculationHistoryAsync(Guid userId);
    Task SaveAsync(Guid userId, SaveCalculationRecordRequest request);
    Task DeleteAsync(Guid userId, Guid recordId);
}
