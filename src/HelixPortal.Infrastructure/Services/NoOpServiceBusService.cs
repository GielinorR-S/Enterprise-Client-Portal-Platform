using HelixPortal.Application.Interfaces.Services;

namespace HelixPortal.Infrastructure.Services;

/// <summary>
/// No-operation Service Bus service for development/testing when Azure Service Bus is not configured.
/// </summary>
public class NoOpServiceBusService : IServiceBusService
{
    public Task PublishRequestCreatedEventAsync(Guid requestId, Guid clientOrganisationId, CancellationToken cancellationToken = default)
    {
        // No-op - just return completed task
        return Task.CompletedTask;
    }

    public Task PublishDocumentUploadedEventAsync(Guid documentId, Guid clientOrganisationId, CancellationToken cancellationToken = default)
    {
        // No-op - just return completed task
        return Task.CompletedTask;
    }
}

