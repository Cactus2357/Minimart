using Microsoft.AspNetCore.Identity;

namespace AuthenticationApi {
    public static class IdentitySeeder {
        public static async Task SeedDefaultAdmin(IServiceProvider serviceProvider) {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string adminRole = "Admin";
            string adminEmail = "admin@example.com";
            string adminPassword = "Admin123$";

            if (!await roleManager.RoleExistsAsync(adminRole))
                await roleManager.CreateAsync(new IdentityRole(adminRole));

            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null) {
                admin = new IdentityUser {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, adminPassword);

                if (!result.Succeeded)
                    throw new Exception("Failed to create default admin: " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            if (!await userManager.IsInRoleAsync(admin, adminRole)) {
                await userManager.AddToRoleAsync(admin, adminRole);
            }
        }

    }
}
