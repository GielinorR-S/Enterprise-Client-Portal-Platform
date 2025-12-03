using HelixPortal.Application.DTOs.Dashboard;
using HelixPortal.Application.Services;
using HelixPortal.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HelixPortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        DashboardService dashboardService,
        ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userIdClaim != null ? Guid.Parse(userIdClaim) : null;
    }

    private Guid? GetCurrentUserOrganisationId()
    {
        var orgIdClaim = User.FindFirst("ClientOrganisationId")?.Value;
        return orgIdClaim != null ? Guid.Parse(orgIdClaim) : null;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetDashboardStats(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var organisationId = GetCurrentUserOrganisationId();

        var stats = await _dashboardService.GetDashboardStatsAsync(
            organisationId,
            userId,
            cancellationToken);

        return Ok(stats);
    }
}

