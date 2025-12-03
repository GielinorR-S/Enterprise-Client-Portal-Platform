// NOTE: The actual JWT Token Service implementation is in:
// HelixPortal.Infrastructure/Services/JwtTokenService.cs
//
// This service implements the ITokenService interface and handles:
// - JWT token generation using HS256 algorithm
// - Token signing with secret key from appsettings.json
// - Claims generation (UserId, Email, DisplayName, Role, ClientOrganisationId)
// - Token expiration configuration
//
// The service is registered in DependencyInjection.cs and injected into
// AuthService for use in authentication flows.
//
// Configuration in appsettings.json:
// {
//   "Jwt": {
//     "SecretKey": "YourSuperSecretKeyForDevelopmentOnly_ChangeInProduction_Minimum32Characters!",
//     "Issuer": "HelixPortal",
//     "Audience": "HelixPortal",
//     "ExpirationMinutes": "1440"
//   }
// }

namespace HelixPortal.Api.Auth;

/// <summary>
/// Documentation reference for JWT Token Service.
/// The actual implementation is in HelixPortal.Infrastructure.Services.JwtTokenService.
/// </summary>
public class JwtTokenServiceDocumentation
{
    // This file exists for documentation purposes only.
    // See HelixPortal.Infrastructure.Services.JwtTokenService for implementation.
}

