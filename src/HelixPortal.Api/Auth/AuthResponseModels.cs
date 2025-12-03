namespace HelixPortal.Api.Auth;

/// <summary>
/// Response model for authentication endpoints (login, register, me).
/// </summary>
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public Guid? ClientOrganisationId { get; set; }
    public Guid UserId { get; set; }
}

/// <summary>
/// Response model for /auth/me endpoint - user profile information.
/// </summary>
public class UserProfileResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public Guid? ClientOrganisationId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

