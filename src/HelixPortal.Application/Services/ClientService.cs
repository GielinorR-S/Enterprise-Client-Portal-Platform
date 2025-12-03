using HelixPortal.Application.DTOs.Client;
using HelixPortal.Application.Interfaces.Repositories;
using HelixPortal.Domain.Entities;

namespace HelixPortal.Application.Services;

/// <summary>
/// Service for handling client organisation operations.
/// </summary>
public class ClientService
{
    private readonly IClientOrganisationRepository _clientOrganisationRepository;

    public ClientService(IClientOrganisationRepository clientOrganisationRepository)
    {
        _clientOrganisationRepository = clientOrganisationRepository;
    }

    public async Task<List<ClientOrganisationDto>> GetAllClientsAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var clients = await _clientOrganisationRepository.GetAllAsync(includeInactive, cancellationToken);
        
        return clients.Select(MapToDto).ToList();
    }

    public async Task<ClientOrganisationDto?> GetClientByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var client = await _clientOrganisationRepository.GetByIdAsync(id, cancellationToken);
        
        if (client == null)
        {
            return null;
        }

        return MapToDto(client);
    }

    public async Task<ClientOrganisationDto> CreateClientAsync(
        CreateClientOrganisationDto dto,
        CancellationToken cancellationToken = default)
    {
        var client = new ClientOrganisation
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Address = dto.Address,
            Timezone = dto.Timezone,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _clientOrganisationRepository.CreateAsync(client, cancellationToken);
        return MapToDto(created);
    }

    public async Task<ClientOrganisationDto?> UpdateClientAsync(
        Guid id,
        CreateClientOrganisationDto dto,
        CancellationToken cancellationToken = default)
    {
        var client = await _clientOrganisationRepository.GetByIdAsync(id, cancellationToken);
        
        if (client == null)
        {
            return null;
        }

        client.Name = dto.Name;
        client.Address = dto.Address;
        client.Timezone = dto.Timezone;
        client.UpdatedAt = DateTime.UtcNow;

        var updated = await _clientOrganisationRepository.UpdateAsync(client, cancellationToken);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteClientAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _clientOrganisationRepository.DeleteAsync(id, cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private ClientOrganisationDto MapToDto(ClientOrganisation client)
    {
        return new ClientOrganisationDto
        {
            Id = client.Id,
            Name = client.Name,
            PrimaryContactId = client.PrimaryContactId,
            PrimaryContactName = client.PrimaryContact?.DisplayName,
            Address = client.Address,
            Timezone = client.Timezone,
            IsActive = client.IsActive,
            CreatedAt = client.CreatedAt
        };
    }
}

