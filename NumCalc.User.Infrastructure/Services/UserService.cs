using Microsoft.Extensions.Logging;
using NumCalc.User.Application.DTOs;
using NumCalc.User.Application.Exceptions;
using NumCalc.User.Application.Interfaces.Repositories;
using NumCalc.User.Application.Interfaces.Services;
using NumCalc.User.Domain.Enums;

namespace NumCalc.User.Infrastructure.Services;

public class UserService(
    IUserRepository userRepository,
    ILogger<UserService> logger) : IUserService
{
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
}