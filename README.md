# HelixPortal - Enterprise Client Portal Platform

A secure, production-ready client portal platform built with Clean Architecture principles using .NET 8, ASP.NET Core, and Azure services.

## Features

- **Client Management**: Manage client organisations and users
- **Document Management**: Upload, view, and download documents with Azure Blob Storage
- **Request/Ticket System**: Create and track support requests with threaded comments
- **Messaging**: Exchange messages on each request
- **Notifications**: Real-time notifications for updates
- **Dashboard**: Overview of requests, documents, and activity
- **Role-Based Access Control**: Client, Staff, and Admin roles with proper authorization

## Architecture

The solution follows **Clean Architecture** principles with clear separation of concerns:

- **HelixPortal.Domain**: Entities, value objects, enums (no dependencies)
- **HelixPortal.Application**: Use cases, DTOs, service interfaces, validation
- **HelixPortal.Infrastructure**: EF Core, repositories, Azure integrations
- **HelixPortal.Api**: REST API controllers and middleware
- **HelixPortal.Web**: MVC web application with Razor views
- **HelixPortal.Tests**: Unit tests with xUnit

## Tech Stack

- **.NET 8**
- **ASP.NET Core** Web API & MVC
- **Entity Framework Core** with SQL Server
- **JWT Authentication**
- **Azure Blob Storage** for documents
- **Azure Service Bus** for async events
- **Azure Key Vault** for secrets
- **FluentValidation** for input validation
- **Serilog** for logging

## Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB or SQL Server)
- Visual Studio 2022 or VS Code
- Azure account (for production deployment)

## Getting Started

### 1. Clone and Restore

```bash
git clone <repository-url>
cd HelixPortal
dotnet restore
```

### 2. Configure Connection Strings

Update `src/HelixPortal.Api/appsettings.json` with your database connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=HelixPortalDb;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

### 3. Run Migrations

```bash
cd src/HelixPortal.Api
dotnet ef migrations add InitialCreate --project ../HelixPortal.Infrastructure --startup-project .
dotnet ef database update --project ../HelixPortal.Infrastructure --startup-project .
```

### 4. Run the Application

**API:**
```bash
cd src/HelixPortal.Api
dotnet run
```

**Web:**
```bash
cd src/HelixPortal.Web
dotnet run
```

The API will be available at `https://localhost:7001` and the Web app at `https://localhost:5001`.

## Default Users

Create your first admin user through the registration endpoint (requires authentication, so you may need to seed data or temporarily allow unauthenticated registration).

## API Documentation

Once running, visit `/swagger` to access the Swagger UI for API documentation.

## Security

- JWT-based authentication
- Role-based authorization (Client, Staff, Admin)
- Client users can only access their own organisation's data
- Strong password requirements for staff/admin users
- Input validation using FluentValidation
- CSRF protection on forms

## Azure Deployment

See `docs/AZURE_DEPLOYMENT.md` for detailed Azure deployment instructions and infrastructure setup.

## Testing

Run unit tests:

```bash
dotnet test
```

## License

[Your License Here]

## Contributing

[Contributing Guidelines]

