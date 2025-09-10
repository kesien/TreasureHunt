using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TreasureHuntApp.Core.Entities;

namespace TreasureHuntApp.Infrastructure.Data;
public static class DbSeeder
{
    public static async Task SeedAsync(TreasureHuntDbContext context, UserManager<UserEntity> userManager)
    {
        if (!await userManager.Users.AnyAsync())
        {
            var adminUser = new UserEntity
            {
                UserName = "admin@treasurehunt.com",
                Email = "admin@treasurehunt.com",
                FullName = "System Administrator",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            await userManager.CreateAsync(adminUser, "Admin123!");
        }

        await context.SaveChangesAsync();
    }
}
