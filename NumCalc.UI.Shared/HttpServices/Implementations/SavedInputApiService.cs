using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.User;
using NumCalc.UI.Shared.Models.User.Enums;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.HttpServices.Implementations;

public class SavedInputApiService(HttpClient httpClient, IAuthStateService authStateService)
    : BaseUserApiService(httpClient, authStateService), ISavedInputApiService
{
    protected override string ApiControllerName => "api/saved-inputs";

    public async Task<List<SavedInputDto>?> GetSavedInputsAsync() =>
        await SendGetRequestAsync<List<SavedInputDto>>($"{ApiControllerName}");

    public async Task<List<SavedInputDto>?> GetLastAsync(int count) =>
        await SendGetRequestAsync<List<SavedInputDto>>($"{ApiControllerName}/last?count={count}");

    public async Task<List<SavedInputDto>?> GetByTypeAsync(CalculationType type) =>
        await SendGetRequestAsync<List<SavedInputDto>>($"{ApiControllerName}?type={type}");
    
    public async Task<SavedInputDto?> CreateSavedInputAsync(CreateSavedInputRequest request) =>
        await SendPostRequestAsync<SavedInputDto>($"{ApiControllerName}", request);

    public async Task DeleteSavedInputAsync(Guid id) =>
        await SendDeleteRequestAsync($"{ApiControllerName}/{id}");
}