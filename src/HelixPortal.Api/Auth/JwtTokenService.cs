using HelixPortal.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HelixPortal.Api.Auth;

/// <summary>
/// JWT token generation service using HS256 symmetric key.
/// Token expires after 7 days.
/// </summary>
public class JwtTokenService
{
    private readonly string _key;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtTokenService(IConfiguration configuration)
    {
        _key = configuration["Jwt:Key"] 
            ?? throw new InvalidOperationException("JWT Key is not configured in appsettings.json");
        _issuer = configuration["Jwt:Issuer"] ?? "HelixPortal";
        _audience = configuration["Jwt:Audience"] ?? "HelixPortalUsers";
    }

    /// <summary>
    /// Generates a JWT token for the specified user.
    /// Token contains: sub (user.Id), email (user.Email), role (user.Role).
    /// Token expires after 7 days.
    /// </summary>
    public string GenerateToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var keyBytes = Encoding.UTF8.GetBytes(_key);

        var claims = new List<Claim>
        {
            new Claim("sub", user.Id.ToString()),           // Subject (user ID)
            new Claim("email", user.Email),                 // User email
            new Claim("role", user.Role.ToString())         // User role
        };

        // Add ClientOrganisationId if user is a client
        if (user.ClientOrganisationId.HasValue)
        {
            claims.Add(new Claim("ClientOrganisationId", user.ClientOrganisationId.Value.ToString()));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7), // 7 days expiry
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(keyBytes),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
