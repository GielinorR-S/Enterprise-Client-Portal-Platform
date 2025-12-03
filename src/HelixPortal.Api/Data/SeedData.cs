using HelixPortal.Domain.Entities;
using HelixPortal.Domain.Enums;
using HelixPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelixPortal.Api.Data;

/// <summary>
/// Database seeding utility to create initial admin user and sample data.
/// </summary>
public static class SeedData
{
    /// <summary>
    /// Seeds the database with initial admin user if no users exist.
    /// Creates admin user with email: admin@helixportal.local, password: Admin123!
    /// </summary>
    public static async Task SeedAdminUserAsync(ApplicationDbContext context)
    {
        // Check if any users exist - if so, skip seeding
        var anyUsersExist = await context.Users.AnyAsync();
        if (anyUsersExist)
        {
            return; // Users already exist, skip seeding
        }

        // Create admin user with securely hashed password
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@helixportal.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!", BCrypt.Net.BCrypt.GenerateSalt(12)), // Securely hashed password
            DisplayName = "Administrator",
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(adminUser);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds the database with sample data for development/testing.
    /// </summary>
    public static async Task SeedSampleDataAsync(ApplicationDbContext context)
    {
        // Only seed if database is empty
        if (await context.ClientOrganisations.AnyAsync())
        {
            return; // Data already exists
        }

        // Create a sample client organisation
        var clientOrg = new ClientOrganisation
        {
            Id = Guid.NewGuid(),
            Name = "Acme Corporation",
            Address = "123 Business St, City, State 12345",
            Timezone = "America/New_York",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.ClientOrganisations.Add(clientOrg);
        await context.SaveChangesAsync();

        // Create a sample client user
        var clientUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "client@acme.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Client@123!"), // Default password
            DisplayName = "Client User",
            Role = UserRole.Client,
            ClientOrganisationId = clientOrg.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(clientUser);
        clientOrg.PrimaryContactId = clientUser.Id;
        await context.SaveChangesAsync();
    }
}

