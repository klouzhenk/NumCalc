using Microsoft.EntityFrameworkCore;
using NumCalc.User.Application.Exceptions;
using NumCalc.User.Application.Interfaces.Repositories;
using NumCalc.User.Domain.Entities;
using NumCalc.User.Domain.Enums;
using NumCalc.User.Infrastructure.Data;

namespace NumCalc.User.Infrastructure.Repositories;

public class CalculationHistoryRepository(AppDbContext dbContext) : ICalculationHistoryRepository
{
    public async Task<CalculationHistoryRecord?> GetByIdAsync(Guid id)
    {
        return await dbContext.CalculationHistoryRecords.FindAsync(id);
    }
    
    public async Task<List<CalculationHistoryRecord>> GetByUserIdAsync(Guid userId)
    {
        return await dbContext.CalculationHistoryRecords
            .Where(record => record.UserId == userId)
            .ToListAsync();
    }

    public async Task<List<CalculationHistoryRecord>> GetLastByUserIdAsync(Guid userId, int count)
    {
        return await dbContext.CalculationHistoryRecords
            .Where(record => record.UserId == userId)
            .OrderByDescending(record => record.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<int> CountByUserIdAsync(Guid userId)
    {
        return await dbContext.CalculationHistoryRecords
            .CountAsync(record => record.UserId == userId);
    }

    public async Task AddAsync(CalculationHistoryRecord record)
    {
        await dbContext.CalculationHistoryRecords.AddAsync(record);
    }

    public async Task DeleteAsync(Guid id)
    {
        var record = await dbContext.CalculationHistoryRecords.FindAsync(id);
        
        if (record is null) 
            throw new CustomException(UserErrorCode.RecordNotFound, $"The record was not found by {id}", 404);
        
        dbContext.CalculationHistoryRecords.Remove(record);
    }

    public async Task DeleteOldestByUserIdAsync(Guid userId)
    {
        var record = await dbContext.CalculationHistoryRecords
            .Where(record => record.UserId == userId)
            .OrderBy(record => record.CreatedAt)
            .FirstOrDefaultAsync();

        if (record is null)
            throw new CustomException(UserErrorCode.RecordNotFound, $"The record was not found for the user with id: {userId}", 404);
        
        dbContext.CalculationHistoryRecords.Remove(record);
    }

    public async Task SaveChangesAsync()
    {
        await dbContext.SaveChangesAsync();
    }
}