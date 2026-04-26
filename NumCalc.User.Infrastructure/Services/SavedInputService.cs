using NumCalc.User.Application.DTOs;
using NumCalc.User.Application.Exceptions;
using NumCalc.User.Application.Interfaces.Repositories;
using NumCalc.User.Application.Interfaces.Services;
using NumCalc.User.Domain.Entities;
using NumCalc.User.Domain.Enums;

namespace NumCalc.User.Infrastructure.Services;

public class SavedInputService(ISavedInputRepository inputRepository) : ISavedInputService
{
    public async Task<List<SavedInputDto>> GetAllAsync(Guid userId)
    {
        var inputData = await inputRepository.GetByUserIdAsync(userId);
        return MapToDto(inputData);
    }

    public async Task<List<SavedInputDto>> GetLastAsync(Guid userId, int count)
    {
        var inputs = await inputRepository.GetLastByUserIdAsync(userId, count);
        return MapToDto(inputs);
    }

    public async Task<List<SavedInputDto>> GetByTypeAsync(Guid userId, CalculationType type)
    {
        var inputData = await inputRepository.GetByUserIdAndTypeAsync(userId, type);
        return MapToDto(inputData);
    }

    private static List<SavedInputDto> MapToDto(List<SavedInput> inputData) =>
        inputData
            .Select(input => new SavedInputDto
            {
                Id = input.Id,
                Name = input.Name,
                Type = input.Type,
                InputsJson = input.InputsJson,
                CreatedAt = input.CreatedAt
            })
            .ToList();

    public async Task<SavedInputDto> CreateAsync(Guid userId, CreateSavedInputRequest request)
    {
        var inputToSave = new SavedInput
        {
            Id = Guid.NewGuid(),
            InputsJson = request.InputsJson,
            Name = request.Name,
            Type = request.Type,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await inputRepository.AddAsync(inputToSave);
        await inputRepository.SaveChangesAsync();

        return new SavedInputDto
        {
            Id = inputToSave.Id,
            Name = inputToSave.Name,
            Type = inputToSave.Type,
            InputsJson = inputToSave.InputsJson,
            CreatedAt = inputToSave.CreatedAt
        };
    }

    public async Task DeleteAsync(Guid userId, Guid inputId)
    {
        var existedInputData = await inputRepository.GetByIdAsync(inputId);
        if (existedInputData is null)
            throw new CustomException(UserErrorCode.RecordNotFound, $"The input data was not found: {inputId}", 404);
        if (existedInputData.UserId != userId)
            throw new CustomException(UserErrorCode.AccessForbidden, $"Access to input data {inputId} is forbidden for user {userId}", 403);

        await inputRepository.DeleteAsync(inputId);
        await inputRepository.SaveChangesAsync();
    }
}