using NumCalc.User.Application.DTOs;
using NumCalc.User.Domain.Enums;

namespace NumCalc.User.Application.Interfaces.Services;

public interface ISavedInputService
{
    Task<List<SavedInputDto>> GetAllAsync(Guid userId);
    Task<List<SavedInputDto>> GetByTypeAsync(Guid userId, CalculationType type);
    Task<SavedInputDto> CreateAsync(Guid userId, CreateSavedInputRequest request);
    Task DeleteAsync(Guid userId, Guid inputId);
}
