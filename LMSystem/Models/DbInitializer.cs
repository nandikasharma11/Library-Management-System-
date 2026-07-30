using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace LMSystem.Models
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndUsersAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // Seed Roles
            string[] roleNames = { "Administrator", "Librarian", "Member" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Seed Users
            var usersToSeed = new (string Email, string Password, string Role)[]
            {
                ("admin@example.com", "Password123", "Administrator"),
                ("librarian@example.com", "Password123", "Librarian"),
                ("member@example.com", "Password123", "Member")
            };

            foreach (var u in usersToSeed)
            {
                var user = await userManager.FindByEmailAsync(u.Email);
                if (user == null)
                {
                    var newUser = new IdentityUser
                    {
                        UserName = u.Email,
                        Email = u.Email,
                        EmailConfirmed = true
                    };

                    var createPowerUser = await userManager.CreateAsync(newUser, u.Password);
                    if (createPowerUser.Succeeded)
                    {
                        await userManager.AddToRoleAsync(newUser, u.Role);
                    }
                }
            }
        }
    }
}
