using HelixPortal.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace HelixPortal.Infrastructure.Services;

/// <summary>
/// No-operation blob storage service for development/testing when Azure Blob Storage is not configured.
/// This allows the application to run locally without Azure Storage.
/// </summary>
public class NoOpBlobStorageService : IBlobStorageService
{
    private readonly ILogger<NoOpBlobStorageService> _logger;

    public NoOpBlobStorageService(ILogger<NoOpBlobStorageService> logger)
    {
        _logger = logger;
    }

    public Task<string> UploadFileAsync(Stream fileStream, string fileName, string containerName, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("NoOpBlobStorageService: UploadFileAsync called. Azure Blob Storage is not configured. File upload will fail.");
        throw new InvalidOperationException("Azure Blob Storage is not configured. Please configure Azure:Storage:ConnectionString in appsettings.json or set AZURE_STORAGE_CONNECTION_STRING environment variable.");
    }

    public Task<Stream> DownloadFileAsync(string blobPath, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("NoOpBlobStorageService: DownloadFileAsync called. Azure Blob Storage is not configured.");
        throw new InvalidOperationException("Azure Blob Storage is not configured. Please configure Azure:Storage:ConnectionString in appsettings.json or set AZURE_STORAGE_CONNECTION_STRING environment variable.");
    }

    public Task DeleteFileAsync(string blobPath, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("NoOpBlobStorageService: DeleteFileAsync called. Azure Blob Storage is not configured.");
        return Task.CompletedTask; // Silently succeed for deletes
    }

    public Task<string> GetFileUrlAsync(string blobPath, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("NoOpBlobStorageService: GetFileUrlAsync called. Azure Blob Storage is not configured.");
        return Task.FromResult(blobPath); // Return the path as-is
    }
}

