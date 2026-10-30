using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using InvoiceManagementSystem.API.Data;
using InvoiceManagementSystem.API.Models;
using InvoiceManagementSystem.API.Services;

namespace InvoiceManagementSystem.API.Extensions;

/// <summary>
/// ServiceExtensions — Metoda extension për të organizuar regjistrimin e shërbimeve.
/// Ndan logjikën e konfigurimit nga Program.cs për ta mbajtur të pastër.
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Regjistron AppDbContext me SQLite si bazë të dhënash.
    /// Connection string lexohet nga appsettings.json.
    /// </summary>
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(
                configuration.GetConnectionString("DefaultConnection"),
                sqliteOptions => sqliteOptions.MigrationsAssembly(
                    typeof(AppDbContext).Assembly.FullName)
            ));

        return services;
    }

    /// <summary>
    /// Regjistron ASP.NET Core Identity me rolet Admin dhe Customer.
    /// Identity menaxhon hashimin e fjalëkalimeve, validimin e përdoruesve
    /// dhe sistemin e roleve.
    /// </summary>
    public static IServiceCollection AddIdentityServices(
        this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            // ── Kërkesat e fjalëkalimit ──────────────────────────────────
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 8;

            // ── Kërkesat e përdoruesit ───────────────────────────────────
            options.User.RequireUniqueEmail = true;

            // ── Kyçja e llogarisë pas tentativave të dështuara ───────────
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        return services;
    }

    /// <summary>
    /// Konfiguron JWT Bearer Authentication.
    /// Çdo kërkesë private duhet të ketë header-in:
    ///   Authorization: Bearer {token}
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtKey = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "Jwt:Key mungon në konfigurim. Shto atë në appsettings.Development.json.");
        var issuer   = configuration["Jwt:Issuer"]!;
        var audience = configuration["Jwt:Audience"]!;

        services
            .AddAuthentication(options =>
            {
                // JWT është skema e paracaktuar — çdo [Authorize] do ta përdorë
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer              = issuer,
                    ValidAudience            = audience,
                    IssuerSigningKey         = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)),
                    // Zero tolerance: token skadon saktësisht kur thotë
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        // Regjistro shërbimet e aplikacionit
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IPdfService, PdfService>();

        return services;
    }

    /// <summary>
    /// Konfiguron CORS (Cross-Origin Resource Sharing) për të lejuar
    /// frontendin React (zakonisht http://localhost:3000) të komunikojë me API-n.
    /// </summary>
    public static IServiceCollection AddCorsPolicy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? ["http://localhost:3000"];

        services.AddCors(options =>
        {
            options.AddPolicy("ReactFrontend", policy =>
            {
                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        return services;
    }

    /// <summary>
    /// Konfiguron OpenAPI me metadatat e API-t.
    /// Scalar UI do t'i shfaqë këto te /scalar/v1.
    /// </summary>
    public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi("v1", options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info = new()
                {
                    Title = "Invoice Management System API",
                    Version = "v1",
                    Description = "ASP.NET Core Web API për menaxhimin e faturave — detyrë rekrutimi."
                };
                return Task.CompletedTask;
            });
        });

        return services;
    }
}
