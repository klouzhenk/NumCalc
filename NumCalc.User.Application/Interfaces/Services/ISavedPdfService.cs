using NumCalc.User.Application.DTOs;

namespace NumCalc.User.Application.Interfaces.Services;

public interface ISavedPdfService
{
    Task<List<SavedPdfMetaDto>> GetAllMetaAsync(Guid userId);
    Task<byte[]> DownloadAsync(Guid userId, Guid pdfId);
    Task SaveAsync(Guid userId, SavePdfRequest request);
    Task DeleteAsync(Guid userId, Guid pdfId);
}
