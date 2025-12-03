namespace HelixPortal.Domain.Enums;

/// <summary>
/// Status of a support request.
/// </summary>
public enum RequestStatus
{
    New = 0,
    InProgress = 1,
    WaitingOnClient = 2,
    Resolved = 3,
    Closed = 4
}

