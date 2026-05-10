using NumCalc.User.Domain.Entities;

namespace NumCalc.User.Application.Interfaces.Repositories;

public interface IPasswordResetTokenRepository
{
    Task<PasswordResetToken?> GetByHashAsync(string hash, CancellationToken ct);
    Task<PasswordResetToken?> GetByUserIdAsync(Guid userId, CancellationToken ct);
    Task AddAsync(PasswordResetToken token);
    void Delete(PasswordResetToken token);
    Task SaveChangesAsync();
}