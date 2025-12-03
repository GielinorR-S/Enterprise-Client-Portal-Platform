namespace HelixPortal.Application.Interfaces.Services;

public interface IBlobStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string containerName, CancellationToken cancellationToken = default);
    Task<Stream> DownloadFileAsync(string blobPath, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string blobPath, CancellationToken cancellationToken = default);
    Task<string> GetFileUrlAsync(string blobPath, CancellationToken cancellationToken = default);
}

