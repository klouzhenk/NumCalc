using NumCalc.Shared.User.DTOs;
using NumCalc.Shared.User.Requests;
using NumCalc.User.Application.Exceptions;
using NumCalc.User.Application.Interfaces.Repositories;
using NumCalc.User.Application.Interfaces.Services;
using NumCalc.User.Domain.Entities;
using NumCalc.User.Domain.Enums;

namespace NumCalc.User.Infrastructure.Services;

public class SavedFileService(ISavedFileRepository fileRepository) : ISavedFileService
{
    public async Task<List<SavedFileMetadataDto>> GetAllMetaAsync(Guid userId)
    {
        var filesMetadata = await fileRepository.GetFilesMetadataByUserIdAsync(userId);

        return filesMetadata
            .Select(metadata => new SavedFileMetadataDto
            {
                Id = metadata.Id,
                FileName = metadata.FileName,
                Type = metadata.Type,
                MethodName = metadata.MethodName,
                CreatedAt = metadata.CreatedAt
            })
            .ToList();
    }

    public async Task<byte[]> DownloadAsync(Guid userId, Guid fileId)
    {
        var file = await fileRepository.GetByIdAsync(fileId);

        if (file is null)
            throw new CustomException(UserErrorCode.RecordNotFound, $"The file was not found: {fileId}", 404);
        if (file.UserId != userId)
            throw new CustomException(UserErrorCode.AccessForbidden, $"Access to file {fileId} is forbidden for user {userId}", 403);

        return file.FileData;
    }

    public async Task SaveAsync(Guid userId, SaveFileRequest request)
    {
        var fileToSave = new SavedFile
        {
            Id = Guid.NewGuid(),
            FileName = request.FileName,
            FileData = request.FileData,
            Type = request.Type,
            MethodName = request.MethodName,
            UserId = userId
        };

        await fileRepository.AddAsync(fileToSave);

        var totalFilesCount = await fileRepository.CountByUserIdAsync(userId);

        if (totalFilesCount >= 10)
        {
            await fileRepository.DeleteOldestByUserIdAsync(userId);
        }
        
        await fileRepository.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid userId, Guid fileId)
    {
        var existedFile = await fileRepository.GetByIdAsync(fileId);

        if (existedFile is null)
            throw new CustomException(UserErrorCode.RecordNotFound, $"The file was not found: {fileId}", 404);
        if (existedFile.UserId != userId)
            throw new CustomException(UserErrorCode.AccessForbidden, $"Access to file {fileId} is forbidden for user {userId}", 403);

        await fileRepository.DeleteAsync(fileId);
        await fileRepository.SaveChangesAsync();
    }
}