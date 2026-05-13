using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.User;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.HttpServices.Implementations;

public class CalculationHistoryApiService(HttpClient httpClient, IAuthStateService authStateService)
    : BaseUserApiService(httpClient, authStateService), ICalculationHistoryApiService
{
    protected override string ApiControllerName => "api/calculation-history";
    
    public async Task<List<CalculationHistoryDto>?> GetHistoryAsync() =>
        await SendGetRequestAsync<List<CalculationHistoryDto>>($"{ApiControllerName}");

    public async Task<List<CalculationHistoryDto>?> GetLastAsync(int count) =>
        await SendGetRequestAsync<List<CalculationHistoryDto>>($"{ApiControllerName}/last?count={count}");

    public async Task SaveHistoryAsync(SaveCalculationRecordRequest request) =>
        await SendPostRequestAsync($"{ApiControllerName}", request);

    public async Task DeleteHistoryAsync(Guid id) =>
        await SendDeleteRequestAsync($"{ApiControllerName}/{id}");
}