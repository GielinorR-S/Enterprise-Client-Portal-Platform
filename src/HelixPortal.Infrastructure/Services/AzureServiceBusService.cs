using Azure.Messaging.ServiceBus;
using HelixPortal.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace HelixPortal.Infrastructure.Services;

/// <summary>
/// Service for publishing messages to Azure Service Bus for async event processing.
/// Events published: RequestCreated, DocumentUploaded
/// </summary>
public class AzureServiceBusService : IServiceBusService
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ILogger<AzureServiceBusService> _logger;
    private readonly string _requestCreatedTopic;
    private readonly string _documentUploadedTopic;

    public AzureServiceBusService(
        ServiceBusClient serviceBusClient,
        ILogger<AzureServiceBusService> logger,
        IConfiguration configuration)
    {
        _serviceBusClient = serviceBusClient;
        _logger = logger;
        _requestCreatedTopic = configuration["Azure:ServiceBus:RequestCreatedTopic"] ?? "request-created";
        _documentUploadedTopic = configuration["Azure:ServiceBus:DocumentUploadedTopic"] ?? "document-uploaded";
    }

    public async Task PublishRequestCreatedEventAsync(
        Guid requestId,
        Guid clientOrganisationId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var messageBody = JsonSerializer.Serialize(new
            {
                RequestId = requestId,
                ClientOrganisationId = clientOrganisationId,
                EventType = "RequestCreated",
                Timestamp = DateTime.UtcNow
            });

            var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody))
            {
                MessageId = Guid.NewGuid().ToString(),
                Subject = "RequestCreated"
            };

            await using var sender = _serviceBusClient.CreateSender(_requestCreatedTopic);
            await sender.SendMessageAsync(message, cancellationToken);

            _logger.LogInformation(
                "RequestCreated event published: RequestId={RequestId}, ClientOrganisationId={ClientOrganisationId}",
                requestId,
                clientOrganisationId);
        }
        catch (Exception ex)
        {
            // Log but don't throw - Service Bus failures shouldn't break the main flow
            _logger.LogWarning(ex, "Failed to publish RequestCreated event for RequestId={RequestId}", requestId);
        }
    }

    public async Task PublishDocumentUploadedEventAsync(
        Guid documentId,
        Guid clientOrganisationId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var messageBody = JsonSerializer.Serialize(new
            {
                DocumentId = documentId,
                ClientOrganisationId = clientOrganisationId,
                EventType = "DocumentUploaded",
                Timestamp = DateTime.UtcNow
            });

            var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody))
            {
                MessageId = Guid.NewGuid().ToString(),
                Subject = "DocumentUploaded"
            };

            await using var sender = _serviceBusClient.CreateSender(_documentUploadedTopic);
            await sender.SendMessageAsync(message, cancellationToken);

            _logger.LogInformation(
                "DocumentUploaded event published: DocumentId={DocumentId}, ClientOrganisationId={ClientOrganisationId}",
                documentId,
                clientOrganisationId);
        }
        catch (Exception ex)
        {
            // Log but don't throw - Service Bus failures shouldn't break the main flow
            _logger.LogWarning(ex, "Failed to publish DocumentUploaded event for DocumentId={DocumentId}", documentId);
        }
    }
}

