using NumCalc.UI.Shared.Models.User;
using NumCalc.UI.Shared.Models.User.Enums;

namespace NumCalc.UI.Shared.HttpServices.Interfaces;

public interface ISavedInputApiService
{
    Task<List<SavedInputDto>?> GetSavedInputsAsync();
    Task<List<SavedInputDto>?> GetByTypeAsync(CalculationType type);
    Task<SavedInputDto?> CreateSavedInputAsync(CreateSavedInputRequest request);
    Task DeleteSavedInputAsync(Guid id);
}