using NumCalc.UI.Shared.Models.User;

namespace NumCalc.UI.Shared.HttpServices.Interfaces;

public interface ISavedFileApiService
{
    Task<List<SavedFileMetadataDto>?> GetSavedFilesAsync();
    Task<byte[]?> DownloadFileAsync(Guid id);
    Task SaveFileAsync(SaveFileRequest request);
    Task DeleteFileAsync(Guid id);
}