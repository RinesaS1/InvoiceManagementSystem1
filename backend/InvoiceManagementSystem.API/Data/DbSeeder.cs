using Microsoft.AspNetCore.Identity;
using InvoiceManagementSystem.API.Models;

namespace InvoiceManagementSystem.API.Data;

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

    private static async Task SeedAdminAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger logger)
    {
        var email    = configuration["AdminSeed:Email"];
        var password = configuration["AdminSeed:Password"];
        var fullName = configuration["AdminSeed:FullName"] ?? "Administrator";

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning(
                "AdminSeed:Email ose AdminSeed:Password mungon në konfigurim. " +
                "Admin nuk u krijua. Shto AdminSeed në appsettings.Development.json " +
                "ose cakto variabla mjedisi.");
            return;
        }

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

