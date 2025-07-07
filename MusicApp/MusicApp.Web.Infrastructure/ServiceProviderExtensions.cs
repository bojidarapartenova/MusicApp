namespace MusicApp.Web.Infrastructure
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;

    using Data.Models;

    using static Common.ApplicationConstants;
    public static class ServiceProviderExtensions
    {
        public static async Task SeedAdminAsync(this IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            bool isAdminRoleExisting = await roleManager
                .RoleExistsAsync(AdminRoleName);
            if (!isAdminRoleExisting)
            {
                await roleManager.CreateAsync(new IdentityRole(AdminRoleName));
            }

            var adminUser = await userManager.FindByEmailAsync(AdminEmail);
            if (adminUser == null)
            {
                var newAdmin = new ApplicationUser()
                {
                    UserName = AdminUsername,
                    Email = AdminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newAdmin, AdminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, AdminRoleName);
                }
            }
        }
    }
}
