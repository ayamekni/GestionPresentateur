using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GestionPresentateur.Models;

namespace GestionPresentateur.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Create roles
            string[] roleNames = { "Admin", "User" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create admin user
            var adminEmail = "admin@cirquefantastique.fr";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "Cirque",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(adminUser, "Admin123!");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // Add sample data...
            if (!context.Roles.Any())
            {
                var roles = new List<Role>
                {
                    new Role { CodeR = "CLW", Libelle = "Clown", Prix = 150.00m },
                    new Role { CodeR = "JON", Libelle = "Jongleur", Prix = 200.00m },
                    new Role { CodeR = "ACR", Libelle = "Acrobate", Prix = 250.00m }
                };

                context.Roles.AddRange(roles);
                await context.SaveChangesAsync();
            }

            if (!context.Presentateurs.Any())
            {
                var presentateurs = new List<Presentateur>
                {
                    new Presentateur { CodeP = "P001", NomP = "Jean Rigolo", CodeR = "CLW" },
                    new Presentateur { CodeP = "P002", NomP = "Marie Adroite", CodeR = "JON" },
                    new Presentateur { CodeP = "P003", NomP = "Paul Voltigeur", CodeR = "ACR" }
                };

                context.Presentateurs.AddRange(presentateurs);
                await context.SaveChangesAsync();
            }
        }
    }
}