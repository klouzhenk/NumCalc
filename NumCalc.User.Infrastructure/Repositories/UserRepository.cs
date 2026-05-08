using Microsoft.EntityFrameworkCore;
using NumCalc.User.Application.Interfaces.Repositories;
using NumCalc.User.Domain.Entities;
using NumCalc.User.Infrastructure.Data;

namespace NumCalc.User.Infrastructure.Repositories;

public class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public async Task<AppUser?> GetByIdAsync(Guid id)
    {
        return await dbContext.Users.FindAsync(id);
    }

    public async Task<AppUser?> GetByUsernameAsync(string username)
    {
        return await dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task AddAsync(AppUser user)
    {
        await dbContext.Users.AddAsync(user);
    }

    public async Task SaveChangesAsync()
    {
        await dbContext.SaveChangesAsync();
    }
}