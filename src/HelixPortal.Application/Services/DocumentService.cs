using HelixPortal.Application.DTOs.Document;
using HelixPortal.Application.Interfaces.Repositories;
using HelixPortal.Application.Interfaces.Services;
using HelixPortal.Domain.Entities;
using HelixPortal.Domain.Enums;

namespace HelixPortal.Application.Services;

/// <summary>
/// Service for handling document operations.
/// </summary>
public class DocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IBlobStorageService _blobStorageService;
    private readonly IServiceBusService _serviceBusService;
    private const string DocumentContainerName = "documents";

    public DocumentService(
        IDocumentRepository documentRepository,
        IBlobStorageService blobStorageService,
        IServiceBusService serviceBusService)
    {
        _documentRepository = documentRepository;
        _blobStorageService = blobStorageService;
        _serviceBusService = serviceBusService;
    }

    public async Task<DocumentDto> UploadDocumentAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        long fileSizeBytes,
        UploadDocumentDto dto,
        Guid uploadedByUserId,
        CancellationToken cancellationToken = default)
    {
        // Generate unique blob path
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var blobPath = await _blobStorageService.UploadFileAsync(
            fileStream,
            uniqueFileName,
            DocumentContainerName,
            cancellationToken);

        var document = new Document
        {
            Id = Guid.NewGuid(),
            ClientOrganisationId = dto.ClientOrganisationId,
            UploadedByUserId = uploadedByUserId,
            FileName = fileName,
            BlobStoragePath = blobPath,
            ContentType = contentType,
            FileSizeBytes = fileSizeBytes,
            Category = Enum.Parse<DocumentCategory>(dto.Category),
            VersionNumber = 1,
            UploadedAt = DateTime.UtcNow
        };

        var created = await _documentRepository.CreateAsync(document, cancellationToken);

        // Publish event to Service Bus
        await _serviceBusService.PublishDocumentUploadedEventAsync(
            created.Id,
            dto.ClientOrganisationId,
            cancellationToken);

        return MapToDto(created);
    }

    public async Task<List<DocumentDto>> GetDocumentsByOrganisationAsync(
        Guid clientOrganisationId,
        CancellationToken cancellationToken = default)
    {
        var documents = await _documentRepository.GetByClientOrganisationIdAsync(
            clientOrganisationId,
            cancellationToken);

        return documents.Select(MapToDto).ToList();
    }

    public async Task<DocumentDto?> GetDocumentAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
        
        if (document == null)
        {
            return null;
        }

        return MapToDto(document);
    }

    public async Task<Stream?> DownloadDocumentAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
        
        if (document == null)
        {
            return null;
        }

        return await _blobStorageService.DownloadFileAsync(document.BlobStoragePath, cancellationToken);
    }

    private DocumentDto MapToDto(Document document)
    {
        return new DocumentDto
        {
            Id = document.Id,
            ClientOrganisationId = document.ClientOrganisationId,
            ClientOrganisationName = document.ClientOrganisation?.Name ?? string.Empty,
            UploadedByUserId = document.UploadedByUserId,
            UploadedByUserName = document.UploadedByUser?.DisplayName ?? string.Empty,
            FileName = document.FileName,
            ContentType = document.ContentType,
            FileSizeBytes = document.FileSizeBytes,
            FileSizeDisplay = FormatFileSize(document.FileSizeBytes),
            Category = document.Category,
            CategoryDisplay = document.Category.ToString(),
            VersionNumber = document.VersionNumber,
            UploadedAt = document.UploadedAt
        };
    }

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}

