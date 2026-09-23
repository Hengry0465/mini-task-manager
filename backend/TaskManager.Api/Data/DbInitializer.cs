using TaskManager.Api.Models;
using TaskManager.Api.Services;

namespace TaskManager.Api.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context, IPasswordHasher passwordHasher)
    {
        await context.Database.EnsureCreatedAsync(); // 仅用于确认数据库连接，实际建表交给 Migration

        var adminEmail = "admin@taskflow.com";
        var exists = context.Users.Any(u => u.Email == adminEmail);

        if (!exists)
        {
            context.Users.Add(new User
            {
                Email = adminEmail,
                PasswordHash = passwordHasher.Hash("Admin@123")
            });

            await context.SaveChangesAsync();
        }
    }
}