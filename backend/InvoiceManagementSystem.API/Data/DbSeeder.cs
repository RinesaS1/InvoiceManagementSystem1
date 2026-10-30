using Microsoft.AspNetCore.Identity;
using InvoiceManagementSystem.API.Models;

namespace InvoiceManagementSystem.API.Data;

/// <summary>
/// DbSeeder — Inicializon bazën e të dhënave me të dhëna fillestare.
///
/// Çfarë bën:
///   1. Krijon rolet "Admin" dhe "Customer" nëse nuk ekzistojnë.
///   2. Krijon llogarinë fillestare Admin nëse nuk ka asnjë Admin.
///
/// SIGURI:
///   - Kredencialet e Admin-it lexohen nga konfigurimi / variablat e mjedisit.
///   - Asnjë fjalëkalim nuk hardkodohet në kod.
///   - Ky seeder ekzekutohet vetëm nëse Admin nuk ekziston.
///   - Në prodhim, caktoni variablat e mjedisit:
///       AdminSeed__Email, AdminSeed__Password, AdminSeed__FullName
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger logger)
    {
        await SeedRolesAsync(roleManager, logger);
        await SeedAdminAsync(userManager, configuration, logger);
    }

    // ─── SEED ROLES ──────────────────────────────────────────────────────────

    private static async Task SeedRolesAsync(
        RoleManager<IdentityRole> roleManager,
        ILogger logger)
    {
        string[] roles = ["Admin", "Customer"];

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
                logger.LogInformation("Roli '{Role}' u krijua.", roleName);
            }
        }
    }

    // ─── SEED ADMIN ───────────────────────────────────────────────────────────

    private static async Task SeedAdminAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger logger)
    {
        // Lexo kredencialet nga konfigurimi (appsettings / env variables)
        var email    = configuration["AdminSeed:Email"];
        var password = configuration["AdminSeed:Password"];
        var fullName = configuration["AdminSeed:FullName"] ?? "Administrator";

        // Nëse seksioni AdminSeed mungon, kalon pa bërë asgjë
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning(
                "AdminSeed:Email ose AdminSeed:Password mungon në konfigurim. " +
                "Admin nuk u krijua. Shto AdminSeed në appsettings.Development.json " +
                "ose cakto variabla mjedisi.");
            return;
        }

        // Nëse ky email ekziston tashmë, mos krijo sërish
        var existingAdmin = await userManager.FindByEmailAsync(email);
        if (existingAdmin is not null)
        {
            logger.LogInformation("Admin me email '{Email}' ekziston tashmë.", email);
            return;
        }

        var admin = new ApplicationUser
        {
            UserName  = email,
            Email     = email,
            FullName  = fullName,
            CreatedAt = DateTime.UtcNow,
            // Konfirmo email-in automatikisht për Admin
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogError("Krijimi i Admin dështoi: {Errors}", errors);
            return;
        }

        await userManager.AddToRoleAsync(admin, "Admin");
        logger.LogInformation(
            "Admin me email '{Email}' u krijua me sukses.", email);
    }
}

