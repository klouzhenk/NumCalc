using NumCalc.UI.Shared.Exceptions;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.User;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.HttpServices.Implementations;

public class SavedFileApiService(HttpClient httpClient, IAuthStateService authStateService)
    : BaseUserApiService(httpClient, authStateService), ISavedFileApiService
{
    protected override string ApiControllerName => "api/saved-files";
    
    public async Task<List<SavedFileMetadataDto>?> GetSavedFilesAsync() =>
        await SendGetRequestAsync<List<SavedFileMetadataDto>>($"{ApiControllerName}");

    public async Task<List<SavedFileMetadataDto>?> GetLastAsync(int count) =>
        await SendGetRequestAsync<List<SavedFileMetadataDto>>($"{ApiControllerName}/last?count={count}");

    public async Task<byte[]?> DownloadFileAsync(Guid id)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiControllerName}/{id}/download");
        ConfigureRequest(request);

        var response = await HttpClient.SendAsync(request);
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsByteArrayAsync()
                ?? throw new ApiException("EMPTY_SERVER_RESPONSE");
        }
        
        var errorMessage = await ExtractErrorMessageAsync(response);
        throw new ApiException(errorMessage);
    }

    public async Task SaveFileAsync(SaveFileRequest request) =>
        await SendPostRequestAsync($"{ApiControllerName}", request);

    public async Task DeleteFileAsync(Guid id) =>
        await SendDeleteRequestAsync($"{ApiControllerName}/{id}");
}