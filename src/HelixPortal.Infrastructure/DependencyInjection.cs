using HelixPortal.Application.Interfaces.Repositories;
using HelixPortal.Application.Interfaces.Services;
using HelixPortal.Infrastructure.Data;
using HelixPortal.Infrastructure.Repositories;
using HelixPortal.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HelixPortal.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure services with dependency injection.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database - Get connection string from configuration
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        // Fallback to environment variable if not in configuration (for Azure deployments)
        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = Environment.GetEnvironmentVariable("AZURE_SQL_CONNECTION_STRING");
        }

        // Ensure connection string is provided
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Database connection string 'DefaultConnection' is not configured. Please set it in appsettings.json or environment variable AZURE_SQL_CONNECTION_STRING.");
        }

        // Register DbContext with SQL Server
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IClientOrganisationRepository, ClientOrganisationRepository>();
        services.AddScoped<IRequestRepository, RequestRepository>();
        services.AddScoped<IRequestCommentRepository, RequestCommentRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();

        // Services
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();

        // Azure Blob Storage
        var blobStorageConnectionString = configuration.GetConnectionString("AzureBlobStorage")
            ?? configuration["Azure:Storage:ConnectionString"]
            ?? Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");

        if (!string.IsNullOrEmpty(blobStorageConnectionString))
        {
            services.AddSingleton(x => new Azure.Storage.Blobs.BlobServiceClient(blobStorageConnectionString));
            services.AddScoped<IBlobStorageService, AzureBlobStorageService>();
        }
        else
        {
            // Use a no-op implementation for development when Azure Storage is not configured
            services.AddScoped<IBlobStorageService, NoOpBlobStorageService>();
        }

        // Azure Service Bus
        var serviceBusConnectionString = configuration.GetConnectionString("AzureServiceBus")
            ?? configuration["Azure:ServiceBus:ConnectionString"]
            ?? Environment.GetEnvironmentVariable("AZURE_SERVICE_BUS_CONNECTION_STRING");

        if (!string.IsNullOrEmpty(serviceBusConnectionString))
        {
            services.AddSingleton(x => new Azure.Messaging.ServiceBus.ServiceBusClient(serviceBusConnectionString));
            services.AddScoped<IServiceBusService, AzureServiceBusService>();
        }
        else
        {
            // Use a no-op implementation for development
            services.AddScoped<IServiceBusService, NoOpServiceBusService>();
        }

        return services;
    }
}

