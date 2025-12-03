namespace HelixPortal.Domain.Enums;

/// <summary>
/// User roles in the system.
/// </summary>
public enum UserRole
{
    /// <summary>Administrator - full system access</summary>
    Admin = 0,
    
    /// <summary>Staff member - can access all client data</summary>
    Staff = 1,
    
    /// <summary>Client user - can only access their organisation's data</summary>
    Client = 2
}

