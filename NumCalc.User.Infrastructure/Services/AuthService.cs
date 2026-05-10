using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
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
    IOptions<WebAppSettings> webAppOptions,
    ILogger<AuthService> logger) : IAuthService
{
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(30);
    
    private readonly WebAppSettings _webApp = webAppOptions.Value;

    public async Task<UserProfileDto> GetCurrentUserAsync(Guid userId)
    {
        var existingUser = await userRepository.GetByIdAsync(userId);
        if (existingUser is null)
        {
            logger.LogWarning("Profile fetch failed — user {UserId} not found", userId);
            throw new CustomException(UserErrorCode.UserNotFound, "User not found", 404);
        }

        return new UserProfileDto
        {
            Username = existingUser.Username,
            Email = existingUser.Email
        };
    }

    public async Task UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
    {
        var existingUser = await userRepository.GetByIdAsync(userId);
        if (existingUser is null)
        {
            logger.LogWarning("Profile update failed — user {UserId} not found", userId);
            throw new CustomException(UserErrorCode.UserNotFound, "User not found", 404);
        }

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, existingUser.PasswordHash);
        if (!isPasswordValid)
        {
            logger.LogWarning("Profile update failed — bad current password for user {UserId}", userId);
            throw new CustomException(UserErrorCode.InvalidCredentials, "Invalid credentials", 401);
        }

        var usernameChanged = false;
        var emailChanged = false;
        var passwordChanged = false;

        if (request.Username is not null && request.Username != existingUser.Username)
        {
            var existingUsername = await userRepository.GetByUsernameAsync(request.Username);
            if (existingUsername is not null && existingUsername.Id != userId)
            {
                logger.LogWarning("Profile update failed for user {UserId} — username {Username} already taken", userId, request.Username);
                throw new CustomException(UserErrorCode.UsernameAlreadyExists, "User with this username already exists", 409);
            }

            existingUser.Username = request.Username;
            usernameChanged = true;
        }

        if (request.Email is not null && request.Email != existingUser.Email)
        {
            var existingEmail = await userRepository.GetByEmailAsync(request.Email);
            if (existingEmail is not null && existingEmail.Id != userId)
            {
                logger.LogWarning("Profile update failed for user {UserId} — email already taken", userId);
                throw new CustomException(UserErrorCode.EmailAlreadyExists, "User with this email already exists", 409);
            }

            existingUser.Email = request.Email;
            emailChanged = true;
        }

        if (request.NewPassword is not null)
        {
            existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            passwordChanged = true;
        }

        await userRepository.SaveChangesAsync();

        logger.LogInformation(
            "Profile updated for user {UserId} (UsernameChanged={UsernameChanged}, EmailChanged={EmailChanged}, PasswordChanged={PasswordChanged})",
            userId, usernameChanged, emailChanged, passwordChanged);
    }

    public async Task DeleteAccountAsync(Guid userId, DeleteAccountRequest request)
    {
        var existingUser = await userRepository.GetByIdAsync(userId);
        if (existingUser is null)
        {
            logger.LogWarning("Account delete failed — user {UserId} not found", userId);
            throw new CustomException(UserErrorCode.UserNotFound, "User not found", 404);
        }

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, existingUser.PasswordHash);
        if (!isPasswordValid)
        {
            logger.LogWarning("Account delete failed — bad current password for user {UserId}", userId);
            throw new CustomException(UserErrorCode.InvalidCredentials, "Invalid credentials", 401);
        }

        userRepository.Delete(existingUser);
        await userRepository.SaveChangesAsync();

        logger.LogInformation("Account deleted for user {UserId}", userId);
    }

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

        logger.LogInformation("User {Username} registered with id {UserId}", user.Username, user.Id);
        return GetAuthResponse(user.Id, user.Username);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var existedUser = await userRepository.GetByUsernameAsync(request.Username);
        if (existedUser is null)
        {
            logger.LogWarning("Login failed — unknown username {Username}", request.Username);
            throw new CustomException(UserErrorCode.InvalidCredentials, "Invalid credentials", 401);
        }

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, existedUser.PasswordHash);
        if (!isPasswordValid)
        {
            logger.LogWarning("Login failed — bad password for {Username}", request.Username);
            throw new CustomException(UserErrorCode.InvalidCredentials, "Invalid credentials", 401);
        }

        logger.LogInformation("User {Username} logged in", existedUser.Username);
        return GetAuthResponse(existedUser.Id, existedUser.Username);
    }

    public async Task RequestPasswordResetAsync(ForgotPasswordRequest request, CancellationToken ct)
    {
        var user = await userRepository.GetByEmailAsync(request.Email);
        if (user is null)
        {
            logger.LogWarning("Password reset requested for unknown email {Email}", request.Email);
            return;
        }

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
        logger.LogInformation("Password reset email sent to user {UserId}", user.Id);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct)
    {
        var hash = HashToken(request.Token);
        var token = await passwordResetTokenRepository.GetByHashAsync(hash, ct);
        if (token is null)
        {
            logger.LogWarning("Password reset attempted with invalid token");
            throw new CustomException(UserErrorCode.InvalidResetToken, "Invalid reset token", 400);
        }

        if (token.ExpiresAt < DateTimeOffset.UtcNow)
        {
            passwordResetTokenRepository.Delete(token);
            await passwordResetTokenRepository.SaveChangesAsync();
            logger.LogWarning("Password reset attempted with expired token for user {UserId}", token.UserId);
            throw new CustomException(UserErrorCode.ExpiredResetToken, "Reset token expired", 400);
        }

        var user = await userRepository.GetByIdAsync(token.UserId)
            ?? throw new CustomException(UserErrorCode.InvalidResetToken, "User not found", 400);

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        passwordResetTokenRepository.Delete(token);
        await userRepository.SaveChangesAsync();

        logger.LogInformation("Password reset completed for user {UserId}", user.Id);
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
        return (raw, HashToken(raw));
    }
    
    private static string HashToken(string raw)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(hashBytes);
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