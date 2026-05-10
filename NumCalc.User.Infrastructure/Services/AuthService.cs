using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NumCalc.User.Application.DTOs;
using NumCalc.User.Application.Exceptions;
using NumCalc.User.Application.Interfaces.Repositories;
using NumCalc.User.Application.Interfaces.Services;
using NumCalc.User.Domain.Entities;
using NumCalc.User.Domain.Enums;
using NumCalc.User.Infrastructure.Configuration;

namespace NumCalc.User.Infrastructure.Services;

public class AuthService(
    IUserRepository userRepository,
    IJwtService jwtService,
    IPasswordResetTokenRepository passwordResetTokenRepository,
    IEmailSender emailSender,
    IOptions<WebAppSettings> webAppOptions) : IAuthService
{
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(30);
    
    private readonly WebAppSettings _webApp = webAppOptions.Value;
    
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

    public async Task RequestPasswordResetAsync(ForgotPasswordRequest request, CancellationToken ct)
    {
        var user = await userRepository.GetByEmailAsync(request.Email);
        if (user is null) return;
        
        var existingToken = await passwordResetTokenRepository.GetByUserIdAsync(user.Id, ct);
        if (existingToken is not null)
            passwordResetTokenRepository.Delete(existingToken);
        
        var (rawToken, hash) = GenerateResetToken();

        await passwordResetTokenRepository.AddAsync(new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = hash,
            ExpiresAt = DateTimeOffset.UtcNow.Add(TokenLifetime)
        });
        await passwordResetTokenRepository.SaveChangesAsync();

        await emailSender.SendAsync(BuildResetEmail(user.Email, rawToken), ct);
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
    
    private EmailMessage BuildResetEmail(string toEmail, string rawToken)
    {
        var resetUrl =
            $"{_webApp.BaseUrl.TrimEnd('/')}/reset-password?token={rawToken}";
        var html = $"""
                    <p>You requested a password reset for your NumCalc
                    account.</p>
                    <p><a href="{resetUrl}">Click here to reset your
                    password</a>.</p>
                    <p>This link expires in 30 minutes.</p>
                    <p>If you didn't request this, you can safely ignore this
                    email.</p>
                    """;
        return new EmailMessage(toEmail, "NumCalc — password reset",
            html);
    }
}