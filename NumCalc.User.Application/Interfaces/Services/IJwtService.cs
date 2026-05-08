namespace NumCalc.User.Application.Interfaces.Services;

public interface IJwtService
{
    string GenerateToken(Guid userId, string username);
}
