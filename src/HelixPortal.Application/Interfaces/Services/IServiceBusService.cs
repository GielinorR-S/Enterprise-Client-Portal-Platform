namespace HelixPortal.Application.Interfaces.Services;

/// <summary>
/// Service for publishing messages to Azure Service Bus for async event processing.
/// </summary>
public interface IServiceBusService
{
    Task PublishRequestCreatedEventAsync(Guid requestId, Guid clientOrganisationId, CancellationToken cancellationToken = default);
    Task PublishDocumentUploadedEventAsync(Guid documentId, Guid clientOrganisationId, CancellationToken cancellationToken = default);
}

