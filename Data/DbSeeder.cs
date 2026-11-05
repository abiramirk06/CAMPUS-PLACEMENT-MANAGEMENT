using CampusPlacementManagement.Models;
using Microsoft.AspNetCore.Identity;

namespace CampusPlacementManagement.Data
{
    public static class DbSeeder
    {
        public static async Task SeedSuperAdminAsync(
            IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

            var userManager = serviceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Create SuperAdmin role if it doesn't exist
            const string roleName = "SuperAdmin";

            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await roleManager.CreateAsync(
                    new IdentityRole(roleName));

                if (!roleResult.Succeeded)
                {
                    throw new Exception(
                        "Failed to create SuperAdmin role: " +
                        string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
            }

            // 2. Create SuperAdmin account if it doesn't exist
            const string adminEmail = "admin@campusplacement.com";
            const string adminPassword = "Admin@12345";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true
                };

                var userResult = await userManager.CreateAsync(
                    adminUser, adminPassword);

                if (!userResult.Succeeded)
                {
                    throw new Exception(
                        "Failed to create SuperAdmin account: " +
                        string.Join(", ", userResult.Errors.Select(e => e.Description)));
                }
            }

            // 3. Assign SuperAdmin role
            if (!await userManager.IsInRoleAsync(adminUser, roleName))
            {
                var roleResult = await userManager.AddToRoleAsync(
                    adminUser, roleName);

                if (!roleResult.Succeeded)
                {
                    throw new Exception(
                        "Failed to assign SuperAdmin role: " +
                        string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}