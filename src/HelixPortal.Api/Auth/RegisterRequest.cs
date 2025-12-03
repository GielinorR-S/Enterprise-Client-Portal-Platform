namespace HelixPortal.Api.Auth;

/// <summary>
/// Request model for user registration.
/// </summary>
public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Role { get; set; } = "Staff"; // Default to Staff
}

