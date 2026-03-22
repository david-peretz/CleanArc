using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArc.Infrastructure.Persistence;

public static class IdentityDataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

        await EnsureRoleAsync(roleManager, "Dispatcher");
        await EnsureRoleAsync(roleManager, "Manager");

        await EnsureUserAsync(userManager, "dispatcher", "Dispatcher123!", "Dispatcher");
        await EnsureUserAsync(userManager, "manager", "Manager123!", "Manager");
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string role)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private static async Task EnsureUserAsync(
        UserManager<IdentityUser> userManager,
        string username,
        string password,
        string role)
    {
        var user = await userManager.FindByNameAsync(username);
        if (user is null)
        {
            user = new IdentityUser
            {
                UserName = username,
                Email = $"{username}@municipality.local",
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to seed user {username}: {string.Join(",", createResult.Errors.Select(x => x.Description))}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            var addRoleResult = await userManager.AddToRoleAsync(user, role);
            if (!addRoleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to add role {role} to user {username}: {string.Join(",", addRoleResult.Errors.Select(x => x.Description))}");
            }
        }
    }
}
