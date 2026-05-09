using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
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

        existedUser = await userRepository.GetByEmailAsync(request.Email);
        if (existedUser is not null)
            throw new CustomException(UserErrorCode.EmailAlreadyExists, "The user already exists by this email", 409);

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
            PasswordHash = passwordHash,
            Email = request.Email
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
    
    private static (string RawToken, string Hash) GenerateResetToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var raw = Base64UrlEncoder.Encode(bytes);
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        var hash = Convert.ToHexString(hashBytes);

        return (raw, hash);
    }
}