using NumCalc.UI.Shared.Exceptions;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.User;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.HttpServices.Implementations;

public class SavedFileApiService(HttpClient httpClient, IAuthStateService authStateService)
    : BaseUserApiService(httpClient, authStateService), ISavedFileApiService
{
    public async Task<List<SavedFileMetadataDto>?> GetSavedFilesAsync() =>
        await SendGetRequestAsync<List<SavedFileMetadataDto>>("api/saved-files");

    public async Task<List<SavedFileMetadataDto>?> GetLastAsync(int count) =>
        await SendGetRequestAsync<List<SavedFileMetadataDto>>($"api/saved-files/last?count={count}");

    public async Task<byte[]?> DownloadFileAsync(Guid id)
    {
        var response = await HttpClient.GetAsync($"api/saved-files/{id}/download");
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsByteArrayAsync() 
                ?? throw new ApiException("EMPTY_SERVER_RESPONSE");
        }
        
        var errorMessage = await ExtractErrorMessageAsync(response);
        throw new ApiException(errorMessage);
    }

    public async Task SaveFileAsync(SaveFileRequest request) =>
        await SendPostRequestAsync("api/saved-files", request);

    public async Task DeleteFileAsync(Guid id) =>
        await SendDeleteRequestAsync($"api/saved-files/{id}");
}