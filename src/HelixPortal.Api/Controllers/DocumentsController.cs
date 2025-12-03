using HelixPortal.Application.DTOs.Document;
using HelixPortal.Application.Services;
using HelixPortal.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HelixPortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly DocumentService _documentService;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(
        DocumentService documentService,
        ILogger<DocumentsController> logger)
    {
        _documentService = documentService;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException("User ID not found in token"));
    }

    private UserRole GetCurrentUserRole()
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        return Enum.Parse<UserRole>(roleClaim ?? "Client");
    }

    private Guid? GetCurrentUserOrganisationId()
    {
        var orgIdClaim = User.FindFirst("ClientOrganisationId")?.Value;
        return orgIdClaim != null ? Guid.Parse(orgIdClaim) : null;
    }

    [HttpPost("upload")]
    [Authorize(Roles = "Staff,Admin")] // Only staff can upload documents
    public async Task<IActionResult> UploadDocument(
        IFormFile file,
        [FromForm] UploadDocumentDto dto,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file uploaded" });
        }

        var userId = GetCurrentUserId();

        try
        {
            using var stream = file.OpenReadStream();
            var document = await _documentService.UploadDocumentAsync(
                stream,
                file.FileName,
                file.ContentType,
                file.Length,
                dto,
                userId,
                cancellationToken);

            return CreatedAtAction(nameof(GetDocument), new { id = document.Id }, document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document");
            return StatusCode(500, new { message = "Error uploading document" });
        }
    }

    [HttpGet]
    [Authorize(Roles = "Client,Staff,Admin")]
    public async Task<IActionResult> GetDocuments(
        [FromQuery] Guid? clientOrganisationId,
        CancellationToken cancellationToken)
    {
        var userRole = GetCurrentUserRole();
        var organisationId = GetCurrentUserOrganisationId();

        // SECURITY: Client users can only see documents from their own organisation
        if (userRole == UserRole.Client)
        {
            clientOrganisationId = organisationId;
        }

        if (!clientOrganisationId.HasValue)
        {
            return BadRequest(new { message = "Client organisation ID is required" });
        }

        var documents = await _documentService.GetDocumentsByOrganisationAsync(
            clientOrganisationId.Value,
            cancellationToken);

        return Ok(documents);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Client,Staff,Admin")]
    public async Task<IActionResult> GetDocument(Guid id, CancellationToken cancellationToken)
    {
        var document = await _documentService.GetDocumentAsync(id, cancellationToken);
        
        if (document == null)
        {
            return NotFound();
        }

        var userRole = GetCurrentUserRole();
        var organisationId = GetCurrentUserOrganisationId();

        // SECURITY: Client users can only see documents from their own organisation
        if (userRole == UserRole.Client && document.ClientOrganisationId != organisationId)
        {
            return Forbid();
        }

        return Ok(document);
    }

    [HttpGet("{id}/download")]
    [Authorize(Roles = "Client,Staff,Admin")]
    public async Task<IActionResult> DownloadDocument(Guid id, CancellationToken cancellationToken)
    {
        var document = await _documentService.GetDocumentAsync(id, cancellationToken);
        
        if (document == null)
        {
            return NotFound();
        }

        var userRole = GetCurrentUserRole();
        var organisationId = GetCurrentUserOrganisationId();

        // SECURITY: Client users can only download documents from their own organisation
        if (userRole == UserRole.Client && document.ClientOrganisationId != organisationId)
        {
            return Forbid();
        }

        var stream = await _documentService.DownloadDocumentAsync(id, cancellationToken);
        
        if (stream == null)
        {
            return NotFound();
        }

        return File(stream, document.ContentType, document.FileName);
    }
}

