using Microsoft.EntityFrameworkCore;
using NumCalc.User.Application.Exceptions;
using NumCalc.User.Application.Interfaces.Repositories;
using NumCalc.User.Domain.Entities;
using NumCalc.User.Domain.Enums;
using NumCalc.User.Infrastructure.Data;

namespace NumCalc.User.Infrastructure.Repositories;

public class SavedFileRepository(AppDbContext dbContext) : ISavedFileRepository
{
    public async Task<List<SavedFile>> GetFilesMetadataByUserIdAsync(Guid userId)
    {
        return await dbContext.SavedFiles
            .Where(file => file.UserId == userId)
            .Select(file => new SavedFile
            {
                Id = file.Id,
                UserId = file.UserId,
                FileName = file.FileName,
                Type = file.Type,
                MethodName = file.MethodName,
                CreatedAt = file.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<SavedFile>> GetLastMetadataByUserIdAsync(Guid userId, int count)
    {
        return await dbContext.SavedFiles
            .Where(file => file.UserId == userId)
            .OrderByDescending(file => file.CreatedAt)
            .Take(count)
            .Select(file => new SavedFile
            {
                Id = file.Id,
                UserId = file.UserId,
                FileName = file.FileName,
                Type = file.Type,
                MethodName = file.MethodName,
                CreatedAt = file.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<SavedFile?> GetByIdAsync(Guid id)
    {
        return await dbContext.SavedFiles.FindAsync(id);
    }

    public async Task<int> CountByUserIdAsync(Guid userId)
    {
        return await dbContext.SavedFiles.CountAsync(file => file.UserId == userId);
    }

    public async Task AddAsync(SavedFile file)
    {
        await dbContext.SavedFiles.AddAsync(file);
    }

    public async Task DeleteAsync(Guid id)
    {
        var file = await dbContext.SavedFiles.FindAsync(id);
        
        if (file is null)
            throw new CustomException(UserErrorCode.RecordNotFound, $"The saved file was not found by {id}", 404);
        
        dbContext.SavedFiles.Remove(file);
    }

    public async Task DeleteOldestByUserIdAsync(Guid userId)
    {
        var file = await dbContext.SavedFiles
            .Where(file => file.UserId == userId)
            .OrderBy(file => file.CreatedAt)
            .FirstOrDefaultAsync();
        
        if (file is null) 
            throw new CustomException(UserErrorCode.RecordNotFound, $"The file was not found for the user with id: {userId}", 404);
        
        dbContext.SavedFiles.Remove(file);
    }

    public async Task SaveChangesAsync()
    {
        await dbContext.SaveChangesAsync();
    }
}