using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using HelixPortal.Application.Interfaces.Services;

namespace HelixPortal.Infrastructure.Services;

/// <summary>
/// Implementation of blob storage service using Azure Blob Storage.
/// </summary>
public class AzureBlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<AzureBlobStorageService> _logger;

    public AzureBlobStorageService(
        BlobServiceClient blobServiceClient,
        ILogger<AzureBlobStorageService> logger)
    {
        _blobServiceClient = blobServiceClient;
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string containerName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get container client and create if it doesn't exist
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

            // Get blob client and upload
            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(fileStream, overwrite: true, cancellationToken);

            _logger.LogInformation("File uploaded successfully: {FileName} to container {ContainerName}", fileName, containerName);

            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName} to container {ContainerName}", fileName, containerName);
            throw;
        }
    }

    public async Task<Stream> DownloadFileAsync(string blobPath, CancellationToken cancellationToken = default)
    {
        try
        {
            // Parse the blob URI to get container and blob name
            var uri = new Uri(blobPath);
            var containerName = uri.Segments[1].TrimEnd('/');
            var blobName = string.Join("", uri.Segments.Skip(2));

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var memoryStream = new MemoryStream();
            await blobClient.DownloadToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;

            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file from {BlobPath}", blobPath);
            throw;
        }
    }

    public async Task DeleteFileAsync(string blobPath, CancellationToken cancellationToken = default)
    {
        try
        {
            var uri = new Uri(blobPath);
            var containerName = uri.Segments[1].TrimEnd('/');
            var blobName = string.Join("", uri.Segments.Skip(2));

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
            
            _logger.LogInformation("File deleted successfully: {BlobPath}", blobPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {BlobPath}", blobPath);
            throw;
        }
    }

    public async Task<string> GetFileUrlAsync(string blobPath, CancellationToken cancellationToken = default)
    {
        // Return the full URL - could be enhanced with SAS tokens for secure access
        return blobPath;
    }
}

