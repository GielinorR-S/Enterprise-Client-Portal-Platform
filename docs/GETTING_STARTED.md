# Getting Started with HelixPortal

This guide will help you get HelixPortal running on your local machine.

## Prerequisites

1. **.NET 8 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **SQL Server** - LocalDB (included with Visual Studio) or SQL Server Express
3. **Visual Studio 2022** (recommended) or **VS Code** with C# extension
4. **Azure Storage Emulator** or **Azure Storage Account** (for document storage)

## Step 1: Clone and Restore

```bash
git clone <repository-url>
cd HelixPortal
dotnet restore
```

## Step 2: Configure Database

1. Update the connection string in `src/HelixPortal.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=HelixPortalDb;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

2. Create and run the initial migration:

```bash
cd src/HelixPortal.Api
dotnet ef migrations add InitialCreate --project ../HelixPortal.Infrastructure --startup-project .
dotnet ef database update --project ../HelixPortal.Infrastructure --startup-project .
```

## Step 3: Configure JWT Secret

1. Generate a secure JWT secret key (at least 32 characters):

```bash
# On Linux/Mac
openssl rand -base64 32

# On Windows PowerShell
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
```

2. Update `src/HelixPortal.Api/appsettings.json`:

```json
{
  "Jwt": {
    "SecretKey": "YOUR_GENERATED_SECRET_KEY_HERE",
    "Issuer": "HelixPortal",
    "Audience": "HelixPortal",
    "ExpirationMinutes": "1440"
  }
}
```

3. Also update `src/HelixPortal.Web/appsettings.json` with the same secret.

## Step 4: Configure Azure Blob Storage (Optional for Development)

For local development, you can:

1. **Option A**: Use Azure Storage Emulator (Azurite)
   ```bash
   npm install -g azurite
   azurite --silent --location c:\azurite --debug c:\azurite\debug.log
   ```

2. **Option B**: Use a real Azure Storage Account
   - Create a storage account in Azure Portal
   - Get the connection string
   - Update `appsettings.json`:
   ```json
   {
     "Azure": {
       "Storage": {
         "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=..."
       }
     }
   }
   ```

3. **Option C**: Leave empty - document uploads will fail, but other features work

## Step 5: Configure Azure Service Bus (Optional)

For local development, the application uses a no-op service bus implementation. You can:

1. Leave it unconfigured (events won't be published)
2. Use Azure Service Bus Emulator (if available)
3. Use a real Azure Service Bus namespace

## Step 6: Run the Application

### Run the API

```bash
cd src/HelixPortal.Api
dotnet run
```

The API will be available at:
- HTTPS: `https://localhost:7001`
- HTTP: `http://localhost:5001`
- Swagger UI: `https://localhost:7001/swagger`

### Run the Web Application

In a new terminal:

```bash
cd src/HelixPortal.Web
dotnet run
```

The Web app will be available at:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`

## Step 7: Create Your First User

Since registration currently requires authentication, you'll need to seed an initial admin user or temporarily modify the registration endpoint.

### Option A: Seed Data (Recommended)

Create a migration or script to seed an admin user:

```csharp
// In a separate seed script or migration
var user = new User
{
    Id = Guid.NewGuid(),
    Email = "admin@helixportal.com",
    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123!"),
    DisplayName = "Admin User",
    Role = UserRole.Admin,
    IsActive = true,
    CreatedAt = DateTime.UtcNow
};
```

### Option B: Temporarily Allow Unauthenticated Registration

Modify `AuthController.Register` to remove `[Authorize]` attribute temporarily, create your user, then restore it.

## Step 8: Access the Application

1. Navigate to `https://localhost:5001`
2. Log in with your created user
3. Explore the dashboard and features

## Testing

Run the unit tests:

```bash
dotnet test
```

## Troubleshooting

### Database Connection Issues

- Ensure SQL Server is running
- Check connection string format
- Verify database exists (migrations should create it)

### JWT Token Issues

- Ensure JWT secret is the same in both API and Web appsettings
- Check token expiration settings
- Verify issuer and audience match

### CORS Issues

- Update CORS settings in `appsettings.json` if using different ports
- Ensure API base URL in Web app matches actual API URL

### Blob Storage Issues

- For development, document uploads may fail if Azure Storage is not configured
- Check connection string format
- Verify container exists (or modify code to auto-create)

## Next Steps

1. Read [SECURITY.md](SECURITY.md) for security best practices
2. Read [AZURE_DEPLOYMENT.md](AZURE_DEPLOYMENT.md) for production deployment
3. Review the code structure and architecture
4. Customize for your specific needs

## Development Tips

- Use Swagger UI (`/swagger`) to test API endpoints
- Check application logs in `logs/` directory
- Use Entity Framework migrations for database changes
- Follow Clean Architecture principles when adding features

## Need Help?

- Check the README.md for architecture overview
- Review the SECURITY.md for security implementation details
- See AZURE_DEPLOYMENT.md for production deployment guidance

