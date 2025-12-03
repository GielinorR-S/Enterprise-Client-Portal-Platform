using HelixPortal.Application.Services;
using HelixPortal.Application.Validators;
using HelixPortal.Infrastructure;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Add Infrastructure layer
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

// Configure JWT Authentication for cookie-based auth in MVC
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey is not configured");
        var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "HelixPortal";
        var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "HelixPortal";

        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(jwtSecretKey))
        };
    })
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddAuthorization();

// Configure API base URL
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7001");
});

// Session for storing auth token
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

