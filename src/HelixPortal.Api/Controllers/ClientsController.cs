using HelixPortal.Application.DTOs.Client;
using HelixPortal.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelixPortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Staff,Admin")] // Only staff/admin can manage clients
public class ClientsController : ControllerBase
{
    private readonly ClientService _clientService;
    private readonly ILogger<ClientsController> _logger;

    public ClientsController(
        ClientService clientService,
        ILogger<ClientsController> logger)
    {
        _clientService = clientService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetClients(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var clients = await _clientService.GetAllClientsAsync(includeInactive, cancellationToken);
        return Ok(clients);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetClient(Guid id, CancellationToken cancellationToken)
    {
        var client = await _clientService.GetClientByIdAsync(id, cancellationToken);
        
        if (client == null)
        {
            return NotFound();
        }

        return Ok(client);
    }

    [HttpPost]
    public async Task<IActionResult> CreateClient(
        [FromBody] CreateClientOrganisationDto dto,
        CancellationToken cancellationToken)
    {
        var client = await _clientService.CreateClientAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetClient), new { id = client.Id }, client);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateClient(
        Guid id,
        [FromBody] CreateClientOrganisationDto dto,
        CancellationToken cancellationToken)
    {
        var client = await _clientService.UpdateClientAsync(id, dto, cancellationToken);
        
        if (client == null)
        {
            return NotFound();
        }

        return Ok(client);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteClient(Guid id, CancellationToken cancellationToken)
    {
        var result = await _clientService.DeleteClientAsync(id, cancellationToken);
        
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}

