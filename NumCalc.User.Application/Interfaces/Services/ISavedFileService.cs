using NumCalc.User.Application.DTOs;

namespace NumCalc.User.Application.Interfaces.Services;

public interface ISavedFileService
{
    Task<List<SavedFileMetadataDto>> GetAllMetaAsync(Guid userId);
    Task<byte[]> DownloadAsync(Guid userId, Guid fileId);
    Task SaveAsync(Guid userId, SaveFileRequest request);
    Task DeleteAsync(Guid userId, Guid fileId);
}
