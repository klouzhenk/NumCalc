using NumCalc.User.Application.DTOs;
using NumCalc.User.Application.Exceptions;
using NumCalc.User.Application.Interfaces.Repositories;
using NumCalc.User.Application.Interfaces.Services;
using NumCalc.User.Domain.Entities;
using NumCalc.User.Domain.Enums;

namespace NumCalc.User.Infrastructure.Services;

public class AuthService(IUserRepository userRepository, IJwtService jwtService) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existedUser = await userRepository.GetByUsernameAsync(request.Username);
        if (existedUser is not null)
            throw new CustomException(UserErrorCode.UsernameAlreadyExists, "The user already exists by this username", 409);

        var user = CreateUser(request);
        await userRepository.AddAsync(user);
        await userRepository.SaveChangesAsync();

        return GetAuthResponse(user.Id, user.Username);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var existedUser = await userRepository.GetByUsernameAsync(request.Username);
        if (existedUser is null)
            throw new CustomException(UserErrorCode.InvalidCredentials, "Invalid credentials", 401);

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, existedUser.PasswordHash);
        if (!isPasswordValid)
            throw new CustomException(UserErrorCode.InvalidCredentials, "Invalid credentials", 401);

        return GetAuthResponse(existedUser.Id, existedUser.Username);
    }
    
    private static AppUser CreateUser(RegisterRequest request)
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        
        return new AppUser
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            PasswordHash = passwordHash
        };
    }

    private AuthResponse GetAuthResponse(Guid userId, string username)
    {
        return new AuthResponse
        {
            Token = jwtService.GenerateToken(userId, username),
            Username = username
        };
    }
}