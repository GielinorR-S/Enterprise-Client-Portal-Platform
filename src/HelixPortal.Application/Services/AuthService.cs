using HelixPortal.Application.DTOs.Auth;
using HelixPortal.Application.Interfaces.Repositories;
using HelixPortal.Application.Interfaces.Services;
using HelixPortal.Domain.Entities;
using HelixPortal.Domain.Enums;

namespace HelixPortal.Application.Services;

/// <summary>
/// Service for handling authentication operations (login, registration).
/// </summary>
public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        
        if (user == null || !user.IsActive)
        {
            return null;
        }

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return null;
        }

        var token = _tokenService.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            Email = user.Email,
            DisplayName = user.DisplayName,
            Role = user.Role.ToString(),
            ClientOrganisationId = user.ClientOrganisationId
        };
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        // Check if user already exists
        var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser != null)
        {
            return null;
        }

        // Only allow Staff or Admin registration for now
        if (!Enum.TryParse<UserRole>(request.Role, out var role) || 
            (role != UserRole.Staff && role != UserRole.Admin))
        {
            return null;
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            DisplayName = request.DisplayName,
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createdUser = await _userRepository.CreateAsync(user, cancellationToken);
        var token = _tokenService.GenerateToken(createdUser);

        return new AuthResponseDto
        {
            Token = token,
            Email = createdUser.Email,
            DisplayName = createdUser.DisplayName,
            Role = createdUser.Role.ToString(),
            ClientOrganisationId = createdUser.ClientOrganisationId
        };
    }
}

