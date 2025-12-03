# Authentication API Documentation

## Overview

The HelixPortal API uses JWT Bearer token authentication with role-based authorization. All authentication endpoints are located under the `/auth` route.

## Authentication Flow

1. **Register** (for Staff/Admin) or use seeded admin account
2. **Login** with credentials to receive JWT token
3. **Include token** in `Authorization: Bearer {token}` header for protected endpoints
4. **Get profile** using `/auth/me` endpoint

## Base URL

All authentication endpoints are prefixed with `/auth`:

```
POST /auth/register
POST /auth/login
GET  /auth/me
```

---

## Endpoints

### 1. Register User

Creates a new user account with hashed password. Validates email uniqueness.

**Endpoint:** `POST /auth/register`

**Authorization:** Required - Staff or Admin role

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "displayName": "John Doe",
  "role": "Staff"
}
```

**Request Fields:**
- `email` (string, required): User email address (must be unique)
- `password` (string, required): Password (minimum 8 characters, must contain uppercase, lowercase, digit, and special character)
- `displayName` (string, required): User's display name
- `role` (string, optional): User role - "Staff" or "Admin" (default: "Staff")

**Response: 200 OK**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "user@example.com",
  "displayName": "John Doe",
  "role": "Staff",
  "clientOrganisationId": null,
  "userId": "123e4567-e89b-12d3-a456-426614174000"
}
```

**Response: 400 Bad Request**
```json
{
  "message": "Email is already registered"
}
```

**Validation Errors:**
- Email format validation
- Password strength requirements (8+ chars, uppercase, lowercase, digit, special char)
- Display name required

**Example cURL:**
```bash
curl -X POST https://localhost:5001/auth/register \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {your_admin_token}" \
  -d '{
    "email": "staff@example.com",
    "password": "SecurePass123!",
    "displayName": "Staff User",
    "role": "Staff"
  }'
```

---

### 2. Login

Authenticates user credentials and returns JWT token.

**Endpoint:** `POST /auth/login`

**Authorization:** Not required (public endpoint)

**Request Body:**
```json
{
  "email": "admin@helixportal.local",
  "password": "Admin123!"
}
```

**Request Fields:**
- `email` (string, required): User email address
- `password` (string, required): User password

**Response: 200 OK**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "admin@helixportal.local",
  "displayName": "Administrator",
  "role": "Admin",
  "clientOrganisationId": null,
  "userId": "123e4567-e89b-12d3-a456-426614174000"
}
```

**Response: 401 Unauthorized**
```json
{
  "message": "Invalid email or password"
}
```

**Token Information:**
- **Algorithm:** HS256 (HMAC SHA-256)
- **Expiration:** 24 hours (1440 minutes) by default
- **Claims included:**
  - `NameIdentifier`: User ID (Guid)
  - `Email`: User email
  - `Name`: Display name
  - `Role`: User role (Admin, Staff, Client)
  - `ClientOrganisationId`: Optional client organisation ID

**Example cURL:**
```bash
curl -X POST https://localhost:5001/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@helixportal.local",
    "password": "Admin123!"
  }'
```

---

### 3. Get Current User Profile

Returns the authenticated user's profile information.

**Endpoint:** `GET /auth/me`

**Authorization:** Required - Valid JWT token

**Headers:**
```
Authorization: Bearer {your_jwt_token}
```

**Response: 200 OK**
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "email": "admin@helixportal.local",
  "displayName": "Administrator",
  "role": "Admin",
  "clientOrganisationId": null,
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z"
}
```

**Response: 401 Unauthorized**
```json
{
  "message": "Invalid token"
}
```

**Response: 404 Not Found**
```json
{
  "message": "User not found"
}
```

**Example cURL:**
```bash
curl -X GET https://localhost:5001/auth/me \
  -H "Authorization: Bearer {your_jwt_token}"
```

---

## Default Admin Account

After database seeding, the following admin account is available:

- **Email:** `admin@helixportal.local`
- **Password:** `Admin123!`
- **Role:** Admin

**Note:** The admin user is only created if no users exist in the database.

---

## JWT Token Configuration

### Token Structure

JWT tokens consist of three parts:
1. **Header:** Algorithm and token type
2. **Payload:** Claims (user information)
3. **Signature:** HMAC signature

### Configuration (appsettings.json)

```json
{
  "Jwt": {
    "SecretKey": "YourSuperSecretKeyForDevelopmentOnly_ChangeInProduction_Minimum32Characters!",
    "Issuer": "HelixPortal",
    "Audience": "HelixPortal",
    "ExpirationMinutes": "1440"
  }
}
```

### Security Recommendations

1. **Production Secret Key:**
   - Use a strong, randomly generated secret key (minimum 32 characters)
   - Store in Azure Key Vault or secure environment variables
   - Never commit secret keys to version control

2. **Token Expiration:**
   - Default: 24 hours (1440 minutes)
   - Adjust based on security requirements
   - Consider implementing refresh tokens for long-lived sessions

3. **HTTPS Only:**
   - Always use HTTPS in production
   - Never send tokens over unencrypted connections

---

## Password Security

### Password Hashing

- **Algorithm:** BCrypt
- **Cost Factor:** 12
- **Salt:** Automatically generated per password
- **Storage:** Never stored in plain text

### Password Requirements

Passwords must meet the following criteria:
- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one digit
- At least one special character

---

## Role-Based Access Control

### User Roles

1. **Admin:** Full system access, can manage all users and organisations
2. **Staff:** Can manage clients, requests, documents, and notifications
3. **Client:** Limited access, can only view/modify their organisation's data

### Authorization Attributes

```csharp
[Authorize]                                    // Any authenticated user
[Authorize(Roles = "Admin")]                  // Admin only
[Authorize(Roles = "Staff,Admin")]            // Staff or Admin
[Authorize(Roles = "Client")]                 // Client users only
```

---

## Error Responses

### Standard Error Format

```json
{
  "message": "Error description"
}
```

### Validation Error Format

```json
[
  {
    "propertyName": "email",
    "errorMessage": "Email is required"
  },
  {
    "propertyName": "password",
    "errorMessage": "Password must be at least 8 characters"
  }
]
```

### Common Status Codes

- **200 OK:** Request successful
- **400 Bad Request:** Invalid input or validation errors
- **401 Unauthorized:** Invalid credentials or missing/invalid token
- **403 Forbidden:** Insufficient permissions
- **404 Not Found:** Resource not found
- **500 Internal Server Error:** Server error

---

## Testing Authentication

### 1. Register a Staff User (as Admin)

```bash
# First, login as admin to get token
TOKEN=$(curl -s -X POST https://localhost:5001/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@helixportal.local","password":"Admin123!"}' \
  | jq -r '.token')

# Register a new staff user
curl -X POST https://localhost:5001/auth/register \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "email": "staff@example.com",
    "password": "Staff123!",
    "displayName": "Staff Member",
    "role": "Staff"
  }'
```

### 2. Login and Get Profile

```bash
# Login
TOKEN=$(curl -s -X POST https://localhost:5001/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@helixportal.local","password":"Admin123!"}' \
  | jq -r '.token')

# Get profile
curl -X GET https://localhost:5001/auth/me \
  -H "Authorization: Bearer $TOKEN"
```

---

## Implementation Details

### Authentication Services

- **Password Hashing:** `HelixPortal.Infrastructure.Services.BcryptPasswordHasher`
- **JWT Token Generation:** `HelixPortal.Infrastructure.Services.JwtTokenService`
- **Authentication Service:** `HelixPortal.Application.Services.AuthService`
- **Authentication Controller:** `HelixPortal.Api.Auth.AuthController`

### Database Seeding

Admin user is automatically seeded on application startup if no users exist:
- Location: `HelixPortal.Api.Data.SeedData.SeedAdminUserAsync()`
- Email: `admin@helixportal.local`
- Password: `Admin123!`
- Role: Admin

### Middleware Pipeline

Authentication is handled by ASP.NET Core middleware:
1. JWT Bearer authentication validates tokens
2. Authorization middleware checks role requirements
3. Global exception handler catches authentication errors

---

## Security Best Practices

1. ✅ Use HTTPS in production
2. ✅ Store JWT secret in secure configuration (Key Vault)
3. ✅ Implement token refresh for long sessions
4. ✅ Validate all inputs with FluentValidation
5. ✅ Hash passwords with BCrypt (cost factor 12)
6. ✅ Enforce strong password requirements
7. ✅ Log authentication failures
8. ✅ Use role-based authorization
9. ✅ Validate email uniqueness on registration
10. ✅ Rate limit login attempts (future enhancement)

---

## Troubleshooting

### Token Invalid or Expired

- Check token expiration time
- Verify secret key matches between token generation and validation
- Ensure token is sent in `Authorization: Bearer {token}` header format

### Login Fails

- Verify user exists and is active
- Check password hash matches
- Ensure email format is correct
- Review server logs for detailed error messages

### Registration Fails

- Verify email is unique
- Check password meets requirements
- Ensure requesting user has Staff/Admin role
- Review validation error messages

---

## References

- [JWT Specification](https://tools.ietf.org/html/rfc7519)
- [BCrypt Algorithm](https://en.wikipedia.org/wiki/Bcrypt)
- [ASP.NET Core Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/)
- [ASP.NET Core Authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/)

