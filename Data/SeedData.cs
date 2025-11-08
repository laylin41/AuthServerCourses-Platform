using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using AuthServer.Models;

namespace AuthServer.Data
{
    public static class SeedData
    {
        public static async Task EnsureSeedData(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            const string adminEmail = "admin@local";
            const string adminPassword = "12345Az_";

            string[] roles = new[] { "Admin", "Professor", "Student" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }

                await userManager.AddToRoleAsync(adminUser, "Student");
                await userManager.AddToRoleAsync(adminUser, "Professor");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
            else
            {
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                    await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
