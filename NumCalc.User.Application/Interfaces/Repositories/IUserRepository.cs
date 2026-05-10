using NumCalc.User.Domain.Entities;

namespace NumCalc.User.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<AppUser?> GetByIdAsync(Guid id);
    Task<AppUser?> GetByEmailAsync(string email);
    Task<AppUser?> GetByUsernameAsync(string username);
    Task AddAsync(AppUser user);
    void Delete(AppUser user);
    Task SaveChangesAsync();
}
