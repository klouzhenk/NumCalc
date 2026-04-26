using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.User;
using NumCalc.UI.Shared.Models.User.Enums;

namespace NumCalc.UI.Shared.HttpServices.Implementations;

public class SavedInputApiService(HttpClient httpClient) : BaseApiService(httpClient), ISavedInputApiService
{
    public async Task<List<SavedInputDto>?> GetSavedInputsAsync() =>
        await SendGetRequestAsync<List<SavedInputDto>>("api/saved-inputs");

    public async Task<List<SavedInputDto>?> GetByTypeAsync(CalculationType type) =>
        await SendGetRequestAsync<List<SavedInputDto>>($"api/saved-inputs?type={type}");
    
    public async Task<SavedInputDto?> CreateSavedInputAsync(CreateSavedInputRequest request) =>
        await SendPostRequestAsync<SavedInputDto>("api/saved-inputs", request);

    public async Task DeleteSavedInputAsync(Guid id) =>
        await SendDeleteRequestAsync($"api/saved-inputs/{id}");
}