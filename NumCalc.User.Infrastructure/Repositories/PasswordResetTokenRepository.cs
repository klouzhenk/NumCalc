using Microsoft.EntityFrameworkCore;
using NumCalc.User.Application.Interfaces.Repositories;
using NumCalc.User.Domain.Entities;
using NumCalc.User.Infrastructure.Data;

namespace NumCalc.User.Infrastructure.Repositories;

public class PasswordResetTokenRepository(AppDbContext dbContext) : IPasswordResetTokenRepository
{
    public Task<PasswordResetToken?> GetByHashAsync(string hash, CancellationToken ct)
    {
        return dbContext.PasswordResetTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);
    }

    public Task<PasswordResetToken?> GetByUserIdAsync(Guid userId, CancellationToken ct)
    {
        return dbContext.PasswordResetTokens.FirstOrDefaultAsync(t => t.UserId == userId, ct);
    }

    public async Task AddAsync(PasswordResetToken token)
    {
        await dbContext.PasswordResetTokens.AddAsync(token);
    }
    
    public void Delete(PasswordResetToken token)
    {
        dbContext.PasswordResetTokens.Remove(token);
    }

    public Task SaveChangesAsync()
    {
        return dbContext.SaveChangesAsync();
    }
}