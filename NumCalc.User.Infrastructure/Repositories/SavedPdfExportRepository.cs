using Microsoft.EntityFrameworkCore;
using NumCalc.User.Application.Interfaces.Repositories;
using NumCalc.User.Domain.Entities;
using NumCalc.User.Infrastructure.Data;

namespace NumCalc.User.Infrastructure.Repositories;

public class SavedPdfExportRepository(AppDbContext dbContext) : ISavedPdfExportRepository
{
    public async Task<List<SavedPdfExport>> GetMetaByUserIdAsync(Guid userId)
    {
        return await dbContext.SavedPdfExports
            .Where(file => file.UserId == userId)
            .Select(file => new SavedPdfExport
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

    public async Task<SavedPdfExport?> GetByIdAsync(Guid id)
    {
        return await dbContext.SavedPdfExports.FindAsync(id);
    }

    public async Task<int> CountByUserIdAsync(Guid userId)
    {
        return await dbContext.SavedPdfExports.CountAsync(file => file.UserId == userId);
    }

    public async Task AddAsync(SavedPdfExport file)
    {
        await dbContext.SavedPdfExports.AddAsync(file);
    }

    public async Task DeleteAsync(Guid id)
    {
        var file = await dbContext.SavedPdfExports.FindAsync(id);
        if (file is null) throw new ArgumentNullException($"The saved PDF file was not found by {id}");
        dbContext.SavedPdfExports.Remove(file);
    }

    public async Task SaveChangesAsync()
    {
        await dbContext.SaveChangesAsync();
    }
}