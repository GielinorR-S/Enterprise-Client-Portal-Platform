# HelixPortal - Complete Run Instructions

This document provides step-by-step instructions to run the HelixPortal application locally.

## Prerequisites

1. **.NET 8 SDK** - Download from [Microsoft](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **SQL Server** - Either:
   - SQL Server LocalDB (included with Visual Studio)
   - SQL Server Express (free)
   - Full SQL Server
3. **Visual Studio 2022** (recommended) or **VS Code** with C# extension
4. **Git** (if cloning from repository)

## Step 1: Verify Prerequisites

Open a terminal/command prompt and verify:

```bash
dotnet --version
# Should show: 8.0.x or higher
```

Verify SQL Server is running (if using full SQL Server):
```bash
# Check SQL Server service is running
```

## Step 2: Clone/Navigate to Solution

```bash
cd "C:\Users\Cini9\Desktop\Portfolio-2026\Enterprise Client Portal Platform"
```

## Step 3: Restore NuGet Packages

```bash
dotnet restore
```

This will download all required NuGet packages.

## Step 4: Configure Connection Strings

### 4.1 Configure API Connection String

Edit `src/HelixPortal.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=HelixPortalDb;Trusted_Connection=true;TrustServerCertificate=true;MultipleActiveResultSets=true"
  }
}
```

**For SQL Server Express:**
```
Server=localhost\\SQLEXPRESS;Database=HelixPortalDb;Trusted_Connection=true;TrustServerCertificate=true;MultipleActiveResultSets=true
```

**For Full SQL Server:**
```
Server=localhost;Database=HelixPortalDb;User Id=your_user;Password=your_password;TrustServerCertificate=true;
```

### 4.2 Configure JWT Secret Key

Generate a secure JWT secret (minimum 32 characters):

**PowerShell:**
```powershell
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
```

**Linux/Mac:**
```bash
openssl rand -base64 32
```

Update both files:
- `src/HelixPortal.Api/appsettings.json`
- `src/HelixPortal.Web/appsettings.json`

Set the `Jwt:SecretKey` value:
```json
{
  "Jwt": {
    "SecretKey": "YOUR_GENERATED_SECRET_KEY_HERE_AT_LEAST_32_CHARACTERS",
    "Issuer": "HelixPortal",
    "Audience": "HelixPortal",
    "ExpirationMinutes": "1440"
  }
}
```

### 4.3 Configure Web App API URL

Edit `src/HelixPortal.Web/appsettings.json`:

```json
{
  "ApiBaseUrl": "https://localhost:7001"
}
```

## Step 5: Create Database Migration

Navigate to the API project directory:

```bash
cd src/HelixPortal.Api
```

Create the initial migration:

```bash
dotnet ef migrations add InitialCreate --project ../HelixPortal.Infrastructure --startup-project .
```

**Expected output:**
```
Build started...
Build succeeded.
Done. To undo this action, use 'dotnet ef migrations remove'
```

## Step 6: Update Database

Apply the migration to create the database:

```bash
dotnet ef database update --project ../HelixPortal.Infrastructure --startup-project .
```

**Expected output:**
```
Build started...
Build succeeded.
Applying migration '20241201000000_InitialCreate'.
Done.
```

This will:
- Create the database if it doesn't exist
- Create all tables (Users, ClientOrganisations, Requests, etc.)

## Step 7: Seed Initial Admin User (Required)

You need to create an initial admin user. You have two options:

### Option A: Create a Seed Script (Recommended)

Create a file `src/HelixPortal.Api/Data/SeedAdminUser.cs`:

```csharp
using HelixPortal.Domain.Entities;
using HelixPortal.Domain.Enums;
using HelixPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public static class SeedAdminUser
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Check if admin user already exists
        var adminExists = await context.Users.AnyAsync(u => u.Email == "admin@helixportal.com");
        if (adminExists) return;

        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@helixportal.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123!"),
            DisplayName = "Admin User",
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(adminUser);
        await context.SaveChangesAsync();
    }
}
```

Then call it in `Program.cs` after migrations:

```csharp
// After migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await SeedAdminUser.SeedAsync(dbContext);
}
```

### Option B: Use SQL Script

Run this SQL script directly on your database:

```sql
USE HelixPortalDb;
GO

INSERT INTO Users (Id, Email, PasswordHash, DisplayName, Role, IsActive, CreatedAt)
VALUES (
    NEWID(),
    'admin@helixportal.com',
    '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5KvPcO7UJdMf2', -- Hash for 'Admin@123!'
    'Admin User',
    2, -- Admin role
    1, -- IsActive
    GETUTCDATE()
);
```

## Step 8: Build the Solution

From the solution root:

```bash
cd ../..  # Back to solution root
dotnet build
```

**Expected output:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

## Step 9: Run the API

In a terminal, navigate to the API project:

```bash
cd src/HelixPortal.Api
dotnet run
```

**Expected output:**
```
info: HelixPortal.Infrastructure.Data.ApplicationDbContext[0]
      Database migration completed successfully
info: Serilog.AspNetCore.RequestLoggingMiddleware[0]
      HelixPortal API starting...
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7001
      Now listening on: http://localhost:5001
```

**Verify API is running:**
- Open browser: `https://localhost:7001/swagger`
- You should see the Swagger UI with all API endpoints

## Step 10: Run the Web Application

In a **NEW terminal**, navigate to the Web project:

```bash
cd src/HelixPortal.Web
dotnet run
```

**Expected output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
      Now listening on: http://localhost:5000
```

## Step 11: Test the Application

### 11.1 Test Login

1. Open browser: `https://localhost:5001`
2. You should see the login page
3. Login with:
   - **Email**: `admin@helixportal.com`
   - **Password**: `Admin@123!`
4. Click "Sign In"
5. You should be redirected to the dashboard

### 11.2 Test API via Swagger

1. Open: `https://localhost:7001/swagger`
2. Expand `POST /api/auth/login`
3. Click "Try it out"
4. Enter:
   ```json
   {
     "email": "admin@helixportal.com",
     "password": "Admin@123!"
   }
   ```
5. Click "Execute"
6. You should receive a JWT token in the response

### 11.3 Test Creating a Request

**Via Swagger:**
1. First, login via `/api/auth/login` to get a token
2. Click "Authorize" button at top
3. Enter: `Bearer YOUR_JWT_TOKEN_HERE`
4. Expand `POST /api/requests`
5. Create a request:
   ```json
   {
     "title": "Test Request",
     "description": "This is a test request",
     "priority": "High",
     "dueDate": "2024-12-31T00:00:00Z"
   }
   ```

**Note**: Client users need to be associated with a ClientOrganisation. For testing, you may need to:
1. Create a ClientOrganisation first (via API or SQL)
2. Create a Client user linked to that organisation

### 11.4 Test Uploading a Document

**Note**: Document uploads will fail if Azure Blob Storage is not configured. You'll see a clear error message.

To test with Azure Storage:
1. Create an Azure Storage Account
2. Get the connection string
3. Add to `appsettings.json`:
   ```json
   {
     "Azure": {
       "Storage": {
         "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=..."
       }
     }
   }
   ```

### 11.5 Test Dashboard Statistics

1. Login to the web app
2. Navigate to Dashboard
3. You should see stats cards (may show zeros if no data)

## Troubleshooting

### Issue: "Database migration failed"

**Solution:**
- Check connection string is correct
- Verify SQL Server is running
- Check database name doesn't already exist (or delete it)
- Try creating migration manually:
  ```bash
  dotnet ef migrations remove --project ../HelixPortal.Infrastructure --startup-project .
  dotnet ef migrations add InitialCreate --project ../HelixPortal.Infrastructure --startup-project .
  dotnet ef database update --project ../HelixPortal.Infrastructure --startup-project .
  ```

### Issue: "JWT SecretKey is not configured"

**Solution:**
- Ensure `appsettings.json` has `Jwt:SecretKey` set
- Key must be at least 32 characters
- Check both API and Web appsettings files

### Issue: "Cannot connect to API from Web app"

**Solution:**
- Verify API is running on `https://localhost:7001`
- Check `ApiBaseUrl` in Web `appsettings.json`
- Check CORS settings in API `appsettings.json`
- Verify HTTPS certificates are trusted

### Issue: "Login fails with 401"

**Solution:**
- Verify admin user was created in database
- Check password hash is correct
- Try resetting password hash:
  ```csharp
  BCrypt.Net.BCrypt.HashPassword("Admin@123!")
  ```
- Update database with new hash

### Issue: "Document upload fails"

**Solution:**
- This is expected if Azure Storage is not configured
- See `NoOpBlobStorageService` will throw an error
- Configure Azure Storage connection string OR
- Implement a local file storage service for development

### Issue: "Build errors"

**Solution:**
- Run `dotnet clean`
- Run `dotnet restore`
- Run `dotnet build` again
- Check for missing packages

## Development Tips

1. **Use Swagger UI** - Great for testing API endpoints
2. **Check Logs** - Logs are in `logs/helixportal-*.txt` (API) and console
3. **Hot Reload** - Use `dotnet watch run` for automatic restarts on code changes
4. **Database Tools** - Use SQL Server Management Studio (SSMS) to inspect database
5. **Postman/Insomnia** - Alternative to Swagger for API testing

## Next Steps

1. **Create Client Organisations** - Use the API to create client organisations
2. **Create Client Users** - Create users linked to organisations
3. **Test Full Workflow** - Create requests, add comments, upload documents
4. **Customize UI** - Modify views and styling
5. **Add Features** - Extend functionality as needed

## Port Configuration

**Default Ports:**
- API: `https://localhost:7001` (HTTPS), `http://localhost:5001` (HTTP)
- Web: `https://localhost:5001` (HTTPS), `http://localhost:5000` (HTTP)

**To Change Ports:**
Edit `Properties/launchSettings.json` in each project.

---

**Last Updated**: December 2024
**For issues or questions**: Check validation report and security documentation

