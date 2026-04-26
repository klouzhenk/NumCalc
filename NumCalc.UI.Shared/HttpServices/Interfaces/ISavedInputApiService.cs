using NumCalc.UI.Shared.Models.User;

namespace NumCalc.UI.Shared.HttpServices.Interfaces;

public interface ISavedInputApiService
{
    Task<List<SavedInputDto>?> GetSavedInputsAsync();
    Task<SavedInputDto?> CreateSavedInputAsync(CreateSavedInputRequest request);
    Task DeleteSavedInputAsync(Guid id);
}