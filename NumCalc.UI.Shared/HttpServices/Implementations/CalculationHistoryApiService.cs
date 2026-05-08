using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.User;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.HttpServices.Implementations;

public class CalculationHistoryApiService(HttpClient httpClient, IAuthStateService authStateService)
    : BaseUserApiService(httpClient, authStateService), ICalculationHistoryApiService
{
    public async Task<List<CalculationHistoryDto>?> GetHistoryAsync() =>
        await SendGetRequestAsync<List<CalculationHistoryDto>>("api/calculation-history");

    public async Task<List<CalculationHistoryDto>?> GetLastAsync(int count) =>
        await SendGetRequestAsync<List<CalculationHistoryDto>>($"api/calculation-history/last?count={count}");

    public async Task SaveHistoryAsync(SaveCalculationRecordRequest request) =>
        await SendPostRequestAsync("api/calculation-history", request);

    public async Task DeleteHistoryAsync(Guid id) =>
        await SendDeleteRequestAsync($"api/calculation-history/{id}");
}