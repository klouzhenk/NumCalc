using NumCalc.UI.Shared.Models.User;

namespace NumCalc.UI.Shared.HttpServices.Interfaces;

public interface ICalculationHistoryApiService
{
    Task<List<CalculationHistoryDto>?> GetHistoryAsync();
    Task<List<CalculationHistoryDto>?> GetLastAsync(int count);
    Task SaveHistoryAsync(SaveCalculationRecordRequest request);
    Task DeleteHistoryAsync(Guid id);
}