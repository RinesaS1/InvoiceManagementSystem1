using Scalar.AspNetCore;
using Microsoft.AspNetCore.Identity;
using InvoiceManagementSystem.API.Data;
using InvoiceManagementSystem.API.Extensions;
using InvoiceManagementSystem.API.Middleware;
using InvoiceManagementSystem.API.Models;

var builder = WebApplication.CreateBuilder(args);

// ─── SHËRBIMET / SERVICES ────────────────────────────────────────────────────

// Kontrollerët — API endpoint-et
builder.Services.AddControllers();

// Baza e të dhënave SQLite me Entity Framework Core
builder.Services.AddDatabase(builder.Configuration);

// ASP.NET Core Identity — menaxhimi i përdoruesve dhe roleve
builder.Services.AddIdentityServices();

// JWT Bearer Authentication + AuthService
builder.Services.AddJwtAuthentication(builder.Configuration);

// CORS — lejo frontendin React të komunikojë me API-n
builder.Services.AddCorsPolicy(builder.Configuration);

// OpenAPI/Scalar — dokumentim dhe testim i API-t
builder.Services.AddOpenApiDocumentation();

// ─── APLIKACIONI / APP PIPELINE ──────────────────────────────────────────────

var app = builder.Build();

// ── Seed: Krijon rolet dhe Admin-in fillestar ────────────────────────────────
// Ekzekutohet çdo herë që starton aplikacioni, por vetëm krijon
// çfarë mungon (idempotent — i sigurt për të ekzekutuar disa herë).
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var config      = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var logger      = scope.ServiceProvider
                          .GetRequiredService<ILogger<Program>>();

    await DbSeeder.SeedAsync(roleManager, userManager, config, logger);
}

// Middleware i gabimeve — duhet të jetë PARA gjithçkaje tjetër
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    // Gjeneron skedarin OpenAPI JSON: /openapi/v1.json
    app.MapOpenApi();

    // Scalar UI — ndërfaqe moderne për dokumentimin e API-t
    // E aksesueshme te: http://localhost:5000/scalar/v1
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Invoice Management System API");
        options.WithTheme(ScalarTheme.DeepSpace);
        options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

// CORS duhet të jetë PARA UseAuthentication/UseAuthorization
app.UseCors("ReactFrontend");

app.UseHttpsRedirection();

// Autentikimi DUHET të jetë para Autorizimit
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Route-i rrënjë — ridrejton te Scalar API UI
app.MapGet("/", () => Results.Redirect("/scalar/v1"))
   .ExcludeFromDescription();

// Endpoint i shëndetit — për të verifikuar që API-ja punon
app.MapGet("/api/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    version = "1.0.0",
    message = "Invoice Management System API është aktiv."
}))
.WithName("HealthCheck")
.WithTags("Health");

app.Run();
