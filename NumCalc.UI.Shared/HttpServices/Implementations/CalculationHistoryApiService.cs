using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.User;

namespace NumCalc.UI.Shared.HttpServices.Implementations;

public class CalculationHistoryApiService(HttpClient httpClient) : BaseApiService(httpClient), ICalculationHistoryApiService
{
    public async Task<List<CalculationHistoryDto>?> GetHistoryAsync() =>
        await SendGetRequestAsync<List<CalculationHistoryDto>>("api/calculation-history");

    public async Task SaveHistoryAsync(SaveCalculationRecordRequest request) =>
        await SendPostRequestAsync("api/calculation-history", request);

    public async Task DeleteHistoryAsync(Guid id) =>
        await SendDeleteRequestAsync($"api/calculation-history/{id}");
}