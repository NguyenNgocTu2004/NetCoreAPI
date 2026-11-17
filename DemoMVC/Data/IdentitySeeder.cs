using Microsoft.AspNetCore.Identity;

namespace DemoMVC.Data;

public static class IdentitySeeder
{
    public const string AdminRole = "Admin";
    public const string DepartmentRole = "DP";
    public const string UserRole = "User";
    private const string LegacyStaffRole = "Staff";

    public static async Task SeedAsync(
        RoleManager<IdentityRole> roleManager,
        UserManager<IdentityUser> userManager,
        IConfiguration configuration)
    {
        foreach (var role in new[] { AdminRole, DepartmentRole, UserRole })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                EnsureSucceeded(result, $"creating the {role} role");
            }
        }

        await MigrateLegacyStaffRoleAsync(roleManager, userManager);

        var admins = await userManager.GetUsersInRoleAsync(AdminRole);
        if (admins.Count > 1)
        {
            throw new InvalidOperationException(
                "More than one Admin account exists. Resolve the duplicate Admin membership before starting the application.");
        }

        if (admins.Count == 1)
        {
            return;
        }

        var username = configuration["Authentication:Admin:Username"];
        var email = configuration["Authentication:Admin:Email"];
        var password = configuration["Authentication:Admin:Password"];
        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Configure the initial Admin account because no Admin user exists yet. " +
                "Use .NET User Secrets or set Authentication__Admin__Username, " +
                "Authentication__Admin__Email, and Authentication__Admin__Password environment variables.");
        }

        var admin = await userManager.FindByNameAsync(username);
        if (admin == null)
        {
            admin = new IdentityUser
            {
                UserName = username,
                Email = email,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(admin, password);
            EnsureSucceeded(createResult, "creating the initial administrator");
        }
        else if (!await userManager.IsInRoleAsync(admin, AdminRole))
        {
            throw new InvalidOperationException(
                $"The configured initial administrator '{username}' already exists but is not in the {AdminRole} role.");
        }

        if (!await userManager.IsInRoleAsync(admin, AdminRole))
        {
            var roleResult = await userManager.AddToRoleAsync(admin, AdminRole);
            EnsureSucceeded(roleResult, "assigning the initial administrator role");
        }
    }

    private static async Task MigrateLegacyStaffRoleAsync(
        RoleManager<IdentityRole> roleManager,
        UserManager<IdentityUser> userManager)
    {
        if (!await roleManager.RoleExistsAsync(LegacyStaffRole))
        {
            return;
        }

        var legacyStaff = await userManager.GetUsersInRoleAsync(LegacyStaffRole);
        foreach (var user in legacyStaff)
        {
            if (!await userManager.IsInRoleAsync(user, DepartmentRole))
            {
                var addResult = await userManager.AddToRoleAsync(user, DepartmentRole);
                EnsureSucceeded(addResult, "migrating a Staff account to the DP role");
            }

            var removeResult = await userManager.RemoveFromRoleAsync(user, LegacyStaffRole);
            EnsureSucceeded(removeResult, "removing the legacy Staff role");
        }
    }

    private static void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Failed {operation}: {errors}");
        }
    }
}
