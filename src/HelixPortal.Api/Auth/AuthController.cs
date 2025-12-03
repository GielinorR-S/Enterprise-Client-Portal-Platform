using HelixPortal.Api.Auth;
using HelixPortal.Application.DTOs.Auth;
using HelixPortal.Application.Interfaces.Repositories;
using HelixPortal.Application.Services;
using HelixPortal.Application.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HelixPortal.Api.Auth;

/// <summary>
/// Authentication controller handling login, registration, and user profile endpoints.
/// </summary>
[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly IUserRepository _userRepository;
    private readonly IValidator<LoginRequestDto> _loginValidator;
    private readonly IValidator<RegisterRequestDto> _registerValidator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        AuthService authService,
        IUserRepository userRepository,
        IValidator<LoginRequestDto> loginValidator,
        IValidator<RegisterRequestDto> registerValidator,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _userRepository = userRepository;
        _loginValidator = loginValidator;
        _registerValidator = registerValidator;
        _logger = logger;
    }

    /// <summary>
    /// Login endpoint - validates credentials and returns JWT token.
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>JWT token and user profile data</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        // Map API model to DTO
        var loginDto = new LoginRequestDto
        {
            Email = request.Email,
            Password = request.Password
        };

        var validationResult = await _loginValidator.ValidateAsync(loginDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var result = await _authService.LoginAsync(loginDto, cancellationToken);
        
        if (result == null)
        {
            _logger.LogWarning("Failed login attempt for email: {Email}", request.Email);
            return Unauthorized(new { message = "Invalid email or password" });
        }

        // Get user to get the UserId
        var user = await _userRepository.GetByEmailAsync(result.Email, cancellationToken);
        if (user == null)
        {
            return Unauthorized(new { message = "User not found" });
        }

        // Map DTO to API response model
        var response = new AuthResponse
        {
            Token = result.Token,
            Email = result.Email,
            DisplayName = result.DisplayName,
            Role = result.Role,
            ClientOrganisationId = result.ClientOrganisationId,
            UserId = user.Id
        };

        return Ok(response);
    }

    /// <summary>
    /// Registration endpoint - creates a new user with hashed password.
    /// Validates email uniqueness.
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>JWT token and user profile data</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        // Map API model to DTO
        var registerDto = new RegisterRequestDto
        {
            Email = request.Email,
            Password = request.Password,
            DisplayName = request.DisplayName,
            Role = request.Role
        };

        var validationResult = await _registerValidator.ValidateAsync(registerDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        // Validate email uniqueness
        var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser != null)
        {
            return BadRequest(new { message = "Email is already registered" });
        }

        var result = await _authService.RegisterAsync(registerDto, cancellationToken);
        
        if (result == null)
        {
            return BadRequest(new { message = "Registration failed. Please check your input and try again." });
        }

        // Map DTO to API response model
        var response = new AuthResponse
        {
            Token = result.Token,
            Email = result.Email,
            DisplayName = result.DisplayName,
            Role = result.Role,
            ClientOrganisationId = result.ClientOrganisationId,
            UserId = Guid.Empty // Will be set after user creation
        };

        // Get the created user to get the ID
        var createdUser = await _userRepository.GetByEmailAsync(result.Email, cancellationToken);
        if (createdUser != null)
        {
            response.UserId = createdUser.Id;
        }

        return Ok(response);
    }

    /// <summary>
    /// Get current user profile - requires authentication.
    /// Reads UserId from JWT token claims.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User profile information</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        // Get user ID from JWT claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        // Fetch user from database
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        var response = new UserProfileResponse
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            Role = user.Role.ToString(),
            ClientOrganisationId = user.ClientOrganisationId,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };

        return Ok(response);
    }
}

