using HelixPortal.Application.DTOs.Request;
using HelixPortal.Application.Services;
using HelixPortal.Application.Validators;
using HelixPortal.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HelixPortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RequestsController : ControllerBase
{
    private readonly RequestService _requestService;
    private readonly IValidator<CreateRequestDto> _createRequestValidator;
    private readonly IValidator<AddCommentDto> _addCommentValidator;
    private readonly ILogger<RequestsController> _logger;

    public RequestsController(
        RequestService requestService,
        IValidator<CreateRequestDto> createRequestValidator,
        IValidator<AddCommentDto> addCommentValidator,
        ILogger<RequestsController> logger)
    {
        _requestService = requestService;
        _createRequestValidator = createRequestValidator;
        _addCommentValidator = addCommentValidator;
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

    [HttpPost]
    [Authorize(Roles = "Client,Staff,Admin")]
    public async Task<IActionResult> CreateRequest([FromBody] CreateRequestDto dto, CancellationToken cancellationToken)
    {
        var validationResult = await _createRequestValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var userId = GetCurrentUserId();
        var organisationId = GetCurrentUserOrganisationId();

        if (organisationId == null)
        {
            return BadRequest(new { message = "User must be associated with a client organisation" });
        }

        var request = await _requestService.CreateRequestAsync(
            dto,
            userId,
            organisationId.Value,
            cancellationToken);

        return CreatedAtAction(nameof(GetRequest), new { id = request.Id }, request);
    }

    [HttpGet]
    [Authorize(Roles = "Client,Staff,Admin")]
    public async Task<IActionResult> GetRequests(
        [FromQuery] Guid? clientOrganisationId,
        [FromQuery] RequestStatus? status,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();
        var organisationId = GetCurrentUserOrganisationId();

        var requests = await _requestService.GetRequestsAsync(
            clientOrganisationId,
            status,
            userId,
            userRole,
            organisationId,
            cancellationToken);

        return Ok(requests);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Client,Staff,Admin")]
    public async Task<IActionResult> GetRequest(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();
        var organisationId = GetCurrentUserOrganisationId();

        var request = await _requestService.GetRequestDetailAsync(
            id,
            userId,
            userRole,
            organisationId,
            cancellationToken);

        if (request == null)
        {
            return NotFound();
        }

        return Ok(request);
    }

    [HttpPost("{id}/comments")]
    [Authorize(Roles = "Client,Staff,Admin")]
    public async Task<IActionResult> AddComment(
        Guid id,
        [FromBody] AddCommentDto dto,
        CancellationToken cancellationToken)
    {
        var validationResult = await _addCommentValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var userId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();
        var organisationId = GetCurrentUserOrganisationId();

        try
        {
            var comment = await _requestService.AddCommentAsync(
                id,
                dto,
                userId,
                userRole,
                organisationId,
                cancellationToken);

            return CreatedAtAction(nameof(GetRequest), new { id }, comment);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> UpdateRequestStatus(
        Guid id,
        [FromBody] UpdateRequestStatusDto dto,
        CancellationToken cancellationToken)
    {
        var userRole = GetCurrentUserRole();

        try
        {
            var request = await _requestService.UpdateRequestStatusAsync(
                id,
                dto.Status,
                userRole,
                cancellationToken);

            if (request == null)
            {
                return NotFound();
            }

            return Ok(request);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }
}

