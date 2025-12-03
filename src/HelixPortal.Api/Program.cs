using HelixPortal.Application.Services;
using HelixPortal.Application.Validators;
using HelixPortal.Api.Data;
using HelixPortal.Infrastructure;
using HelixPortal.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/helixportal-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HelixPortal API",
        Version = "v1",
        Description = "Secure client portal API for document management, requests, and messaging"
    });

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure JWT Authentication
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey is not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "HelixPortal";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "HelixPortal";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey))
    };
});

builder.Services.AddAuthorization();

// Add Infrastructure layer (Database, Repositories, Services)
builder.Services.AddInfrastructure(builder.Configuration);

// Add Application layer services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<RequestService>();
builder.Services.AddScoped<DocumentService>();
builder.Services.AddScoped<ClientService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<DashboardService>();

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestDtoValidator>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", policy =>
    {
        policy.WithOrigins(builder.Configuration["Cors:AllowedOrigins"]?.Split(',') ?? new[] { "http://localhost:5000" })
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowWebApp");

// Global exception handling middleware
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ensure database is created/migrated and seeded
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        
        if (!string.IsNullOrEmpty(connectionString))
        {
            dbContext.Database.Migrate();
            Log.Information("Database migration completed successfully");

            // Seed initial admin user
            await SeedData.SeedAdminUserAsync(dbContext);
            Log.Information("Database seeding completed");

            // Seed sample data in development
            if (app.Environment.IsDevelopment())
            {
                await SeedData.SeedSampleDataAsync(dbContext);
                Log.Information("Sample data seeding completed");
            }
        }
        else
        {
            Log.Warning("Database connection string is not configured. Migrations will be skipped.");
        }
    }
}
catch (Exception ex)
{
    Log.Error(ex, "Error running database migrations or seeding. The application will continue but database may not be ready.");
    // Don't throw - allow the app to start even if migrations fail
}

Log.Information("HelixPortal API starting...");

app.Run();

