using NumCalc.User.Application.DTOs;

namespace NumCalc.User.Application.Interfaces.Services;

public interface ISavedInputService
{
    Task<List<SavedInputDto>> GetAllAsync(Guid userId);
    Task<SavedInputDto> CreateAsync(Guid userId, CreateSavedInputRequest request);
    Task DeleteAsync(Guid userId, Guid inputId);
}
