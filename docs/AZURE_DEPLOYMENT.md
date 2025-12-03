# Azure Deployment Guide for HelixPortal

This guide explains the Azure resources required for deploying HelixPortal and how to configure them.

## Required Azure Resources

### 1. App Service (for API and Web)

**Purpose**: Hosts the HelixPortal.Api and HelixPortal.Web applications

**Configuration**:
- **App Service Plan**: Standard S1 or higher (recommended: P1V2 for production)
- **Runtime Stack**: .NET 8
- **Operating System**: Windows or Linux

**Key Settings**:
- Always On: Enabled
- HTTPS Only: Enabled
- Minimum TLS Version: 1.2

**Connection Strings**:
- `DefaultConnection`: Azure SQL Database connection string
- `AzureBlobStorage`: Azure Storage connection string
- `AzureServiceBus`: Service Bus connection string

**Application Settings**:
- `Jwt:SecretKey`: Store in Key Vault (reference via `@Microsoft.KeyVault(...)`)
- `Azure:ServiceBus:RequestCreatedTopic`: Topic name for request events
- `Azure:ServiceBus:DocumentUploadedTopic`: Topic name for document events

### 2. Azure SQL Database

**Purpose**: Stores all application data (users, requests, documents, notifications)

**Configuration**:
- **Service Tier**: Standard S2 or higher (recommended: Premium P1 for production)
- **Compute Size**: 50 DTU minimum (adjust based on load)
- **Backup Retention**: 7-35 days (recommended: 35 days)

**Connection String Format**:
```
Server=tcp:{server-name}.database.windows.net,1433;Initial Catalog={database-name};Persist Security Info=False;User ID={username};Password={password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

**Security**:
- Enable Azure AD authentication (optional but recommended)
- Configure firewall rules to allow App Service outbound IPs
- Enable Transparent Data Encryption (TDE)

### 3. Azure Storage Account

**Purpose**: Stores uploaded documents via Azure Blob Storage

**Configuration**:
- **Performance**: Standard
- **Replication**: LRS for development, GRS for production
- **Access Tier**: Hot

**Container**:
- Create a container named `documents`
- Set access level to Private (no public access)

**Connection String Format**:
```
DefaultEndpointsProtocol=https;AccountName={account-name};AccountKey={account-key};EndpointSuffix=core.windows.net
```

### 4. Azure Service Bus

**Purpose**: Publishes events for async processing (RequestCreated, DocumentUploaded)

**Configuration**:
- **Namespace**: Create a standard or premium tier namespace
- **Topics**: Create two topics:
  - `request-created`
  - `document-uploaded`
- **Subscriptions**: Create subscriptions as needed for consumers

**Connection String Format**:
```
Endpoint=sb://{namespace}.servicebus.windows.net/;SharedAccessKeyName={policy-name};SharedAccessKey={key}
```

### 5. Azure Key Vault

**Purpose**: Stores sensitive configuration (connection strings, JWT secrets, API keys)

**Configuration**:
- **Access Policy**: Grant access to App Service managed identity
- **Secrets**: Store the following:
  - SQL Database connection string
  - Storage Account connection string
  - Service Bus connection string
  - JWT Secret Key

**Integration**:
- Enable managed identity on App Service
- Grant Key Vault access to the managed identity
- Reference secrets in App Service configuration using:
  ```
  @Microsoft.KeyVault(SecretUri=https://{vault-name}.vault.azure.net/secrets/{secret-name}/)
  ```

## Infrastructure Setup Options

### Option 1: ARM Template (Recommended for Production)

Create an ARM template or use Azure Bicep to deploy all resources in one go. This ensures consistency and version control.

### Option 2: Azure Portal (Quick Start)

Manually create resources through the Azure Portal for initial testing.

### Option 3: Terraform

Use Terraform for infrastructure-as-code if your organization uses it.

## Deployment Steps

### 1. Create Resource Group

```bash
az group create --name HelixPortal-RG --location eastus
```

### 2. Deploy Infrastructure

Use your preferred method (ARM/Bicep/Terraform) to create:
- SQL Database and Server
- Storage Account
- Service Bus Namespace and Topics
- Key Vault
- App Service Plan and App Services

### 3. Configure App Service

1. **Enable Managed Identity**:
   - Go to App Service → Identity → System assigned → On

2. **Configure Key Vault Access**:
   - Go to Key Vault → Access policies
   - Add the App Service managed identity with Get/List permissions

3. **Set Connection Strings** (in App Service Configuration):
   ```
   DefaultConnection = @Microsoft.KeyVault(SecretUri=https://{vault}.vault.azure.net/secrets/sql-connection/)
   AzureBlobStorage = @Microsoft.KeyVault(SecretUri=https://{vault}.vault.azure.net/secrets/storage-connection/)
   AzureServiceBus = @Microsoft.KeyVault(SecretUri=https://{vault}.vault.azure.net/secrets/servicebus-connection/)
   ```

4. **Set Application Settings**:
   ```
   Jwt__SecretKey = @Microsoft.KeyVault(SecretUri=https://{vault}.vault.azure.net/secrets/jwt-secret/)
   ASPNETCORE_ENVIRONMENT = Production
   ```

### 4. Run Database Migrations

Deploy and run EF Core migrations:

```bash
# Using Azure CLI or Kudu console
dotnet ef database update --project HelixPortal.Infrastructure --startup-project HelixPortal.Api
```

Or use Azure DevOps pipeline (see below).

### 5. Deploy Application

Deploy the API and Web applications using:
- Azure DevOps CI/CD pipeline (recommended)
- Visual Studio Publish
- Azure CLI
- GitHub Actions

## Azure DevOps Pipeline Example

See `azure-pipelines.yml` in the repository root for a complete CI/CD pipeline example.

## Cost Estimation (Monthly, Approximate)

- **App Service Plan (P1V2)**: ~$150
- **SQL Database (Premium P1)**: ~$500
- **Storage Account (Standard LRS, 100GB)**: ~$2
- **Service Bus (Standard, 1M operations)**: ~$10
- **Key Vault (Standard)**: ~$1
- **Total**: ~$663/month

*Note: Costs vary by region and usage. Use Azure Pricing Calculator for accurate estimates.*

## Security Checklist

- [ ] Enable HTTPS only on App Service
- [ ] Configure firewall rules on SQL Database
- [ ] Use managed identity for Key Vault access
- [ ] Enable Transparent Data Encryption on SQL Database
- [ ] Configure CORS in API appsettings
- [ ] Store all secrets in Key Vault
- [ ] Enable Application Insights for monitoring
- [ ] Configure backup policies for SQL Database
- [ ] Enable audit logging
- [ ] Review and set appropriate access policies

## Monitoring and Maintenance

- **Application Insights**: Enable for both API and Web apps
- **Alerts**: Set up alerts for:
  - High error rates
  - Slow response times
  - Database DTU usage
  - Storage account capacity
- **Backups**: Configure automated backups for SQL Database
- **Logging**: Review Application Insights logs regularly

## Troubleshooting

### Common Issues

1. **Database Connection Failures**:
   - Check SQL Server firewall rules
   - Verify connection string in Key Vault
   - Ensure managed identity has access

2. **Blob Storage Access Denied**:
   - Verify storage account connection string
   - Check container access level
   - Ensure App Service can access storage account

3. **Service Bus Connection Issues**:
   - Verify connection string
   - Check topic names match configuration
   - Ensure topics exist in namespace

4. **Key Vault Access Denied**:
   - Verify managed identity is enabled
   - Check Key Vault access policies
   - Ensure secret names match references

## Additional Resources

- [Azure App Service Documentation](https://docs.microsoft.com/azure/app-service/)
- [Azure SQL Database Documentation](https://docs.microsoft.com/azure/sql-database/)
- [Azure Blob Storage Documentation](https://docs.microsoft.com/azure/storage/blobs/)
- [Azure Service Bus Documentation](https://docs.microsoft.com/azure/service-bus-messaging/)
- [Azure Key Vault Documentation](https://docs.microsoft.com/azure/key-vault/)

