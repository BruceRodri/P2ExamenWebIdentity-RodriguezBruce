using Microsoft.AspNetCore.Identity;

namespace NorthwindApp.Data;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

        string[] roles = { "Admin", "Employee" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        string emailAdmin = "admin@espe.edu.ec";
        string passwordAdmin = "Admin123*";

        var admin = await userManager.FindByEmailAsync(emailAdmin);

        if (admin == null)
        {
            admin = new IdentityUser
            {
                UserName = emailAdmin,
                Email = emailAdmin,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(admin, passwordAdmin);
        }

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
            await userManager.AddToRoleAsync(admin, "Admin");
    }
}
