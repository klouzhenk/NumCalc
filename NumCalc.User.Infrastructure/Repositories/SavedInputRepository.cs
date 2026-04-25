using Microsoft.EntityFrameworkCore;
using NumCalc.User.Application.Exceptions;
using NumCalc.User.Application.Interfaces.Repositories;
using NumCalc.User.Domain.Entities;
using NumCalc.User.Domain.Enums;
using NumCalc.User.Infrastructure.Data;

namespace NumCalc.User.Infrastructure.Repositories;

public class SavedInputRepository(AppDbContext dbContext) : ISavedInputRepository
{
    public async Task<List<SavedInput>> GetByUserIdAsync(Guid userId)
    {
        return await dbContext.SavedInputs
            .Where(record => record.UserId == userId)
            .ToListAsync();
    }

    public async Task<SavedInput?> GetByIdAsync(Guid id)
    {
        return await dbContext.SavedInputs.FindAsync(id);
    }

    public async Task AddAsync(SavedInput input)
    {
        await dbContext.SavedInputs.AddAsync(input);
    }

    public async Task DeleteAsync(Guid id)
    {
        var record = await dbContext.SavedInputs.FindAsync(id);
        
        if (record is null)
            throw new CustomException(UserErrorCode.RecordNotFound, $"The saved input was not found by {id}", 404);
        
        dbContext.SavedInputs.Remove(record);
    }

    public async Task SaveChangesAsync()
    {
        await dbContext.SaveChangesAsync();
    }
}