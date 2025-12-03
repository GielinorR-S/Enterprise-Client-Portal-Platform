# Security Documentation for HelixPortal

This document outlines the security measures implemented in HelixPortal and where key security checks occur.

## Authentication and Authorization

### JWT Token-Based Authentication
- **Location**: `HelixPortal.Api/Program.cs` - JWT Bearer authentication configured
- All API endpoints (except `/api/auth/login`) require a valid JWT token
- Token contains: User ID, Email, Display Name, Role, Client Organisation ID

### Role-Based Authorization

Three roles are defined:
1. **Client**: Can only access their own organisation's data
2. **Staff**: Can access all client data
3. **Admin**: Full system access

**Authorization Attributes**:
- `[Authorize]` - Requires authentication
- `[Authorize(Roles = "Staff,Admin")]` - Requires Staff or Admin role
- Applied at controller level in `HelixPortal.Api/Controllers/*`

### Security Check Locations

#### 1. Request Authorization
**File**: `HelixPortal.Application/Services/RequestService.cs`

```csharp
// SECURITY: Client users can only see requests from their own organisation
if (currentUserRole == UserRole.Client && request.ClientOrganisationId != currentUserOrganisationId)
{
    return null;
}
```

#### 2. Document Authorization
**File**: `HelixPortal.Application/Services/DocumentService.cs`
**File**: `HelixPortal.Api/Controllers/DocumentsController.cs`

- Client users can only download documents from their own organisation
- Checked in controller before allowing download

#### 3. Comment Visibility
**File**: `HelixPortal.Application/Services/RequestService.cs`

- Internal comments (IsInternal = true) are hidden from client users
- Only staff/admin can create internal comments

#### 4. Status Updates
**File**: `HelixPortal.Application/Services/RequestService.cs`

```csharp
// SECURITY: Only staff/admin can change request status
if (currentUserRole == UserRole.Client)
{
    throw new UnauthorizedAccessException("Only staff can update request status");
}
```

## Password Security

### Strong Password Requirements
**File**: `HelixPortal.Application/Validators/RegisterRequestDtoValidator.cs`

Password must:
- Be at least 8 characters
- Contain at least one uppercase letter
- Contain at least one lowercase letter
- Contain at least one digit
- Contain at least one special character

**Enforcement**: FluentValidation in registration endpoint

### Password Hashing
**File**: `HelixPortal.Infrastructure/Services/BcryptPasswordHasher.cs`

- Uses BCrypt with cost factor of 12
- Passwords are never stored in plain text
- Hashing occurs during user registration

## Input Validation

### FluentValidation Rules
All DTOs have validators:
- `LoginRequestDtoValidator.cs` - Email format, required fields
- `RegisterRequestDtoValidator.cs` - Strong password, email format
- `CreateRequestDtoValidator.cs` - Title/description length, priority values
- `AddCommentDtoValidator.cs` - Message length

**Location**: `HelixPortal.Application/Validators/`

### Model Validation
- API controllers validate DTOs using FluentValidation
- Invalid requests return 400 Bad Request with error details

## CSRF Protection

### Web Forms
**Location**: `HelixPortal.Web/Views/*`

- ASP.NET Core MVC automatically includes anti-forgery tokens
- Forms use `@Html.AntiForgeryToken()` (implicit in form helpers)
- Validate with `[ValidateAntiForgeryToken]` attribute

**Note**: For JWT-based API, CSRF is less of a concern as tokens are in headers, not cookies. However, web forms still use anti-forgery tokens.

## SQL Injection Prevention

### Entity Framework Core
- All database queries use parameterized queries via EF Core
- No raw SQL with string concatenation
- Repository pattern abstracts database access

**Location**: `HelixPortal.Infrastructure/Repositories/*`

## Data Isolation

### Organisation-Level Filtering
**Pattern**: All queries filter by `ClientOrganisationId` for client users

**Example Locations**:
- `RequestService.GetRequestsAsync()` - Filters by organisation
- `DocumentService.GetDocumentsByOrganisationAsync()` - Organisation-specific
- `DashboardService.GetDashboardStatsAsync()` - Organisation-scoped stats

**Security Principle**: Always check organisation ID matches current user's organisation for client users.

## Secret Management

### Configuration
- Production secrets stored in Azure Key Vault
- Connection strings never hardcoded
- JWT secret key retrieved from Key Vault

**Configuration File**: `appsettings.Production.json` references Key Vault

### Environment Variables
- Sensitive values can be set via environment variables
- Used in Azure App Service configuration

## Logging and Monitoring

### Security Event Logging
**File**: `HelixPortal.Api/Controllers/AuthController.cs`

```csharp
_logger.LogWarning("Failed login attempt for email: {Email}", request.Email);
```

- Failed login attempts are logged
- Security-relevant events logged for audit trail

## HTTPS Enforcement

### Production Configuration
- App Service configured for HTTPS only
- TLS 1.2 minimum required
- Configured in Azure App Service settings

## Session Security

### Web Application Sessions
**Location**: `HelixPortal.Web/Program.cs`

- Session cookies marked as HttpOnly
- Session timeout: 60 minutes
- Token stored in session (not in client-side storage)

## Security Checklist

### Implementation Status

- [x] JWT authentication implemented
- [x] Role-based authorization
- [x] Organisation-level data isolation
- [x] Strong password requirements
- [x] Input validation (FluentValidation)
- [x] Parameterized queries (EF Core)
- [x] Password hashing (BCrypt)
- [x] CSRF tokens on forms
- [x] Security event logging
- [x] HTTPS configuration guide
- [ ] Rate limiting (TODO: Consider adding)
- [ ] IP whitelisting (Optional: For extra security)
- [ ] Two-factor authentication (Future enhancement)

## Recommendations

1. **Rate Limiting**: Implement rate limiting on authentication endpoints to prevent brute force attacks
2. **Audit Logging**: Enhance logging to include all data access events
3. **Token Refresh**: Consider implementing refresh tokens for longer sessions
4. **IP Restrictions**: Optionally restrict admin access by IP address
5. **Security Headers**: Add security headers (HSTS, X-Frame-Options, etc.) via middleware
6. **Regular Security Audits**: Review access logs and authentication patterns regularly

## Reporting Security Issues

If you discover a security vulnerability, please report it responsibly by contacting the development team rather than opening a public issue.

