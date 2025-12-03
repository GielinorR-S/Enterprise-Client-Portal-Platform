# HelixPortal Project Structure

This document explains the structure and organization of the HelixPortal solution.

## Solution Overview

HelixPortal follows **Clean Architecture** principles with clear separation of concerns:

```
HelixPortal/
├── src/
│   ├── HelixPortal.Domain/          # Core domain entities and business rules
│   ├── HelixPortal.Application/     # Use cases and application logic
│   ├── HelixPortal.Infrastructure/  # External concerns (database, Azure, etc.)
│   ├── HelixPortal.Api/             # REST API controllers
│   └── HelixPortal.Web/             # MVC web application
├── src/
│   └── HelixPortal.Tests/           # Unit tests
├── docs/                            # Documentation
├── azure-pipelines.yml             # CI/CD pipeline
└── README.md                        # Main documentation
```

## Project Details

### HelixPortal.Domain

**Purpose**: Core domain model - no external dependencies

**Contents**:
- `Entities/` - Domain entities (User, Request, Document, etc.)
- `Enums/` - Enumeration types (UserRole, RequestStatus, etc.)

**Key Files**:
- `Entities/User.cs` - User entity
- `Entities/Request.cs` - Support request entity
- `Entities/Document.cs` - Document metadata entity
- `Enums/UserRole.cs` - User roles (Client, Staff, Admin)

**Dependencies**: None (pure domain)

---

### HelixPortal.Application

**Purpose**: Application business logic and use cases

**Contents**:
- `DTOs/` - Data transfer objects for API/UI
- `Interfaces/` - Repository and service interfaces
- `Services/` - Application services (business logic)
- `Validators/` - FluentValidation validators

**Key Services**:
- `Services/AuthService.cs` - Authentication logic
- `Services/RequestService.cs` - Request management
- `Services/DocumentService.cs` - Document handling
- `Services/DashboardService.cs` - Dashboard statistics
- `Services/ClientService.cs` - Client organisation management
- `Services/NotificationService.cs` - Notification handling

**Dependencies**: HelixPortal.Domain

---

### HelixPortal.Infrastructure

**Purpose**: External integrations and data persistence

**Contents**:
- `Data/` - EF Core DbContext and configurations
- `Repositories/` - Repository implementations
- `Services/` - Infrastructure services (Azure, hashing, tokens)

**Key Components**:
- `Data/ApplicationDbContext.cs` - EF Core context
- `Data/Configurations/` - Entity configurations
- `Repositories/` - All repository implementations
- `Services/AzureBlobStorageService.cs` - Azure Blob Storage integration
- `Services/AzureServiceBusService.cs` - Service Bus messaging
- `Services/BcryptPasswordHasher.cs` - Password hashing
- `Services/JwtTokenService.cs` - JWT token generation

**Dependencies**: HelixPortal.Application, HelixPortal.Domain

---

### HelixPortal.Api

**Purpose**: REST API endpoints

**Contents**:
- `Controllers/` - API controllers
- `Middleware/` - Global exception handling
- `Program.cs` - Startup configuration

**Key Controllers**:
- `AuthController.cs` - Authentication endpoints
- `RequestsController.cs` - Request CRUD and comments
- `DocumentsController.cs` - Document upload/download
- `ClientsController.cs` - Client management (admin)
- `NotificationsController.cs` - Notification endpoints
- `DashboardController.cs` - Dashboard statistics

**Features**:
- JWT authentication
- Swagger/OpenAPI documentation
- Global exception handling
- CORS configuration

**Dependencies**: HelixPortal.Application, HelixPortal.Infrastructure

---

### HelixPortal.Web

**Purpose**: MVC web application for user interface

**Contents**:
- `Controllers/` - MVC controllers
- `Views/` - Razor views
- `wwwroot/` - Static files (CSS, JS, images)

**Key Controllers**:
- `HomeController.cs` - Dashboard
- `AccountController.cs` - Login/logout
- Additional controllers for Requests, Documents, etc.

**UI Features**:
- Bootstrap 5 styling
- Sidebar navigation
- Responsive design
- Toast notifications

**Dependencies**: HelixPortal.Application, HelixPortal.Infrastructure

---

### HelixPortal.Tests

**Purpose**: Unit tests

**Contents**:
- `Services/` - Service layer tests
- Tests use xUnit, Moq, and FluentAssertions

**Test Files**:
- `Services/RequestServiceTests.cs` - Request service tests
- `Services/DashboardServiceTests.cs` - Dashboard service tests

**Testing Strategy**:
- Mock repositories
- Test business logic in Application layer
- AAA pattern (Arrange, Act, Assert)

**Dependencies**: HelixPortal.Application, HelixPortal.Domain

---

## Data Flow

### Request Flow Example

1. **Web/API Layer** (`HelixPortal.Web` or `HelixPortal.Api`)
   - Controller receives HTTP request
   - Validates input using FluentValidation
   - Calls Application service

2. **Application Layer** (`HelixPortal.Application`)
   - Service contains business logic
   - Uses repository interfaces (not implementations)
   - Returns DTOs

3. **Infrastructure Layer** (`HelixPortal.Infrastructure`)
   - Repository implementation uses EF Core
   - Accesses database via DbContext
   - Returns domain entities

4. **Domain Layer** (`HelixPortal.Domain`)
   - Pure entities with no dependencies
   - Contains business rules

## Key Architectural Patterns

### Dependency Inversion

- Application layer defines interfaces
- Infrastructure layer implements interfaces
- Dependency injection wires everything together

### Repository Pattern

- Abstracts data access
- Makes business logic testable
- Swappable implementations

### CQRS-Like Separation

- Queries return DTOs (not entities)
- Commands use domain entities
- Clear separation of read/write models

### Service Layer Pattern

- Business logic in services
- Controllers are thin
- Services orchestrate repositories

## Adding New Features

### Step 1: Domain
1. Add entity to `HelixPortal.Domain/Entities/`
2. Add enum if needed to `HelixPortal.Domain/Enums/`

### Step 2: Infrastructure
1. Add EF Core configuration in `HelixPortal.Infrastructure/Data/Configurations/`
2. Create repository interface in `HelixPortal.Application/Interfaces/Repositories/`
3. Implement repository in `HelixPortal.Infrastructure/Repositories/`

### Step 3: Application
1. Create DTOs in `HelixPortal.Application/DTOs/`
2. Create validators in `HelixPortal.Application/Validators/`
3. Create service in `HelixPortal.Application/Services/`
4. Register service in DI container

### Step 4: API
1. Create controller in `HelixPortal.Api/Controllers/`
2. Add authorization attributes
3. Call application service

### Step 5: Web (if needed)
1. Create controller in `HelixPortal.Web/Controllers/`
2. Create views in `HelixPortal.Web/Views/`
3. Add routing if needed

### Step 6: Tests
1. Create test file in `HelixPortal.Tests/Services/`
2. Mock repositories
3. Test business logic

## Configuration Files

- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development overrides
- `appsettings.Production.json` - Production settings (typically empty, uses Key Vault)

## Database Migrations

Migrations are stored in:
- `HelixPortal.Infrastructure/Migrations/` (generated by EF Core)

To create a migration:
```bash
dotnet ef migrations add MigrationName --project src/HelixPortal.Infrastructure --startup-project src/HelixPortal.Api
```

## Logging

- Uses Serilog for structured logging
- Logs to console and file
- Configuration in `appsettings.json` under `Serilog` section

## Security

- JWT authentication
- Role-based authorization
- Input validation with FluentValidation
- Password hashing with BCrypt
- See `docs/SECURITY.md` for details

## Azure Integration

- **Blob Storage**: Document storage (`AzureBlobStorageService`)
- **Service Bus**: Event publishing (`AzureServiceBusService`)
- **Key Vault**: Secret management (via configuration)

All Azure services are optional for local development (no-op implementations available).

