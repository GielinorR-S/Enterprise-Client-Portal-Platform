# Authentication Module Implementation Summary

## Files Created/Modified

### 1. Authentication Folder Structure (`src/HelixPortal.Api/Auth/`)

#### Created Files:
- **AuthController.cs** - Main authentication controller with three endpoints
  - POST /auth/register - Register new user
  - POST /auth/login - Login and get JWT token
  - GET /auth/me - Get current user profile (requires authentication)

- **LoginRequest.cs** - Request model for login endpoint
  - Email (string)
  - Password (string)

- **RegisterRequest.cs** - Request model for registration endpoint
  - Email (string)
  - Password (string)
  - DisplayName (string)
  - Role (string, default: "Staff")

- **AuthResponse.cs** - Response models for authentication endpoints
  - AuthResponse - Contains token, user info
  - UserProfileResponse - User profile information

- **PasswordHasher.cs** - Password hashing service
  - Uses HMACSHA256 algorithm
  - Generates random salt for each password
  - Stores salt + hash as base64 string
  - Verifies passwords with constant-time comparison

- **JwtTokenService.cs** - JWT token generation service
  - Uses HS256 symmetric key
  - Token expires after 7 days
  - Contains claims: sub (userId), email, role
  - Configurable issuer and audience

### 2. Configuration Files Modified

- **Program.cs** (`src/HelixPortal.Api/Program.cs`)
  - Added authentication services registration
  - Configured JWT Bearer authentication
  - Added PasswordHasher and JwtTokenService to DI
  - Configured token validation parameters
  - Set NameClaimType to "sub" and RoleClaimType to "role"

- **appsettings.json** (`src/HelixPortal.Api/appsettings.json`)
  - Updated JWT configuration:
    - Changed "SecretKey" to "Key"
    - Set "Issuer" to "HelixPortal"
    - Set "Audience" to "HelixPortalUsers"
    - Removed "ExpirationMinutes" (hardcoded to 7 days in service)

- **SeedData.cs** (`src/HelixPortal.Api/Data/SeedData.cs`)
  - Updated to use new PasswordHasher instead of BCrypt
  - Creates admin user with email: admin@helixportal.local
  - Password: Admin123!

### 3. Entity Framework Configuration

- **UserConfiguration.cs** (already exists)
  - PasswordHash property configured with max length 500
  - Role property uses HasConversion<int>() for enum storage
  - Email has unique index

## Implementation Details

### Password Hashing
- **Algorithm**: HMACSHA256
- **Salt**: 32 bytes (256 bits), randomly generated
- **Hash**: 32 bytes (256 bits)
- **Storage**: Base64 encoded string containing salt + hash (64 bytes total)
- **Security**: Constant-time comparison to prevent timing attacks

### JWT Token Configuration
- **Algorithm**: HS256 (HMAC SHA-256)
- **Key Source**: appsettings.json "Jwt:Key"
- **Expiration**: 7 days from generation
- **Claims**:
  - `sub`: User ID (Guid as string)
  - `email`: User email address
  - `role`: User role (Admin, Staff, Client)
  - `ClientOrganisationId`: Optional, only for client users
- **Issuer**: HelixPortal
- **Audience**: HelixPortalUsers

### Endpoints

1. **POST /auth/register**
   - Validates email uniqueness
   - Hashes password using PasswordHasher
   - Creates user in database
   - Returns JWT token and user info

2. **POST /auth/login**
   - Validates email and password
   - Verifies password using PasswordHasher
   - Returns JWT token and user info

3. **GET /auth/me**
   - Requires [Authorize] attribute
   - Reads user ID from JWT "sub" claim
   - Fetches user from database
   - Returns user profile information

## Security Features

1. ✅ Password hashing with HMACSHA256 and random salt
2. ✅ Constant-time password verification
3. ✅ JWT token expiration (7 days)
4. ✅ Email uniqueness validation
5. ✅ Role-based authorization support
6. ✅ Secure token validation in middleware

## Swagger Integration

All endpoints are automatically discovered by Swagger and will appear in:
- `/swagger` UI
- Swagger JSON at `/swagger/v1/swagger.json`
- JWT Bearer authentication is configured for Swagger

## Testing

### Default Admin Account
- Email: `admin@helixportal.local`
- Password: `Admin123!`
- Role: Admin

### Test Endpoints

1. **Register a user:**
   ```
   POST /auth/register
   {
     "email": "test@example.com",
     "password": "Test123!",
     "displayName": "Test User",
     "role": "Staff"
   }
   ```

2. **Login:**
   ```
   POST /auth/login
   {
     "email": "admin@helixportal.local",
     "password": "Admin123!"
   }
   ```

3. **Get profile:**
   ```
   GET /auth/me
   Authorization: Bearer {token}
   ```

## Dependencies

All required services are registered in Program.cs:
- `PasswordHasher` - Scoped service
- `JwtTokenService` - Scoped service
- `IUserRepository` - Registered in Infrastructure layer

## Notes

- The authentication module is completely self-contained in the `/Auth` folder
- No external dependencies on Application layer DTOs (uses its own request/response models)
- Password hashing uses HMACSHA256 as specified (not BCrypt)
- Token expiry is hardcoded to 7 days in JwtTokenService
- All endpoints follow RESTful conventions

