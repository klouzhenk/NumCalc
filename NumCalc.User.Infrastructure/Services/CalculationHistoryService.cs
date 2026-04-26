using NumCalc.User.Application.DTOs;
using NumCalc.User.Application.Exceptions;
using NumCalc.User.Application.Interfaces.Repositories;
using NumCalc.User.Application.Interfaces.Services;
using NumCalc.User.Domain.Entities;
using NumCalc.User.Domain.Enums;

namespace NumCalc.User.Infrastructure.Services;

public class CalculationHistoryService(ICalculationHistoryRepository calculationHistoryRepository) : ICalculationHistoryService
{
    public async Task<List<CalculationHistoryDto>> GetCalculationHistoryAsync(Guid userId)
    {
        var historyData = await calculationHistoryRepository.GetByUserIdAsync(userId);

        return historyData
            .Select(record => new CalculationHistoryDto()
            {
                Id = record.Id,
                Type = record.Type,
                MethodName = record.MethodName,
                InputsJson = record.InputsJson,
                ResultSummary = record.ResultSummary,
                ExecutionTimeMs = record.ExecutionTimeMs,
                CreatedAt = record.CreatedAt
            })
            .ToList();
    }

    public async Task SaveAsync(Guid userId, SaveCalculationRecordRequest request)
    {
        var record = new CalculationHistoryRecord
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = request.Type,
            MethodName = request.MethodName,
            InputsJson = request.InputsJson,
            ResultSummary = request.ResultSummary,
            ExecutionTimeMs = request.ExecutionTimeMs
        };

        await calculationHistoryRepository.AddAsync(record);

        var totalCount = await calculationHistoryRepository.CountByUserIdAsync(userId);

        if (totalCount >= 100)
        {
            await calculationHistoryRepository.DeleteOldestByUserIdAsync(userId);
        }

        await calculationHistoryRepository.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid userId, Guid recordId)
    {
        var record = await calculationHistoryRepository.GetByIdAsync(recordId);
        if (record is null)
            throw new CustomException(UserErrorCode.RecordNotFound, $"The record was not found: {recordId}", 404);
        if (record.UserId != userId)
            throw new CustomException(UserErrorCode.AccessForbidden, $"Access to record {recordId} is forbidden for user {userId}", 403);

        await calculationHistoryRepository.DeleteAsync(recordId);
        await calculationHistoryRepository.SaveChangesAsync();
    }
}