using HelixPortal.Domain.Entities;

namespace HelixPortal.Application.Interfaces.Services;

public interface ITokenService
{
    string GenerateToken(User user);
}

