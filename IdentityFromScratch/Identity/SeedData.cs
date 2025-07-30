using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace IdentityFromScratch.Identity;

internal static class SeedData
{
    /// <summary>
    /// Seeds default identity data such as roles and an admin user.
    /// Remember, this method should be called only once, ideally during the first application startup. REMOVE this method immediately after the first run to prevent duplicate seeding and data breach.
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    internal static async Task SeedIdentityDefaultDataAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<User<int>>>();

        var isAdminRoleExists = await roleManager.RoleExistsAsync("Admin");
        // Skip seeding if roles already exist
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            var defaultRoles = new[] { "Admin", "User" };

            Log.Information("Seeding default roles...");
            foreach (var role in defaultRoles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(role));
                    Log.Debug($"Role {role} is created successfully.");
                }
            }
        }

        Log.Debug("Check whether the admin user is exist.");
        var result = await userManager.GetUsersInRoleAsync("Admin");
        if (result!.Count == 0)
        {
            Log.Debug("No admin user found, make a new one.");
            var firstAdminUser = new User<int>
            {
                UserName = "Admin",
                Email = "example@example.com",
                EmailConfirmed = true,
                FirstName = "John",
                LastName = "Doe",
                JoinedDate = DateTime.UtcNow,
            };

            // Admin user creation
            Log.Information("Initializing a new admin user...");
            var creationResult = await userManager.CreateAsync(firstAdminUser, "PleaseChangeThisPassword!@#1");

            if (creationResult.Succeeded)
            {
                Log.Information("Grant the admin user the Admin role.");
                await userManager.AddToRoleAsync(firstAdminUser, "Admin");
            }
            else
            {
                Log.Error("Failed to create the admin user. Errors: {Errors}", string.Join(", ", creationResult.Errors.Select(e => e.Description)));
                throw new InvalidOperationException("Failed to create the admin user.");
            }
            Log.Information("Admin user created successfully. Don't forget to exclude this code.");
        }

    }

    internal static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            try
            {
                var services = scope.ServiceProvider;
                var dbContext = services.GetRequiredService<AppDbContext>();

                await SeedIdentityDefaultDataAsync(services);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to initialize the database.");
                throw new HostAbortedException(ex.Message, ex);
            }
        }
    }
}
