// NOTE: The actual Password Hasher implementation is in:
// HelixPortal.Infrastructure/Services/BcryptPasswordHasher.cs
//
// This service implements the IPasswordHasher interface and handles:
// - Password hashing using BCrypt with cost factor of 12
// - Password verification against stored hashes
// - Secure salt generation for each password
//
// The service is registered in DependencyInjection.cs and injected into
// AuthService for use in registration and login flows.
//
// Usage:
// - HashPassword(password) - returns BCrypt hash
// - VerifyPassword(password, hash) - returns true if password matches hash
//
// Security:
// - Uses BCrypt.Net library
// - Cost factor of 12 provides good security/performance balance
// - Each password gets a unique salt automatically

namespace HelixPortal.Api.Auth;

/// <summary>
/// Documentation reference for Password Hasher Service.
/// The actual implementation is in HelixPortal.Infrastructure.Services.BcryptPasswordHasher.
/// </summary>
public class PasswordHasherDocumentation
{
    // This file exists for documentation purposes only.
    // See HelixPortal.Infrastructure.Services.BcryptPasswordHasher for implementation.
}

