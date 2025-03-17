/*//sample to seed data
using Domain.Entities;
using System.Linq;
using Utilities.Security;
namespace Infrastructure.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureCreated();
            if (!context.Users.Any())
            {
                context.Users.Add(new User
                {
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@example.com",
                    PasswordHash = Utilities.Security.PasswordHelper.HashPassword("admin123"),
                    Role = "Admin"
                });
                context.SaveChanges();
            }
        }
    }
}




*/






using Domain.Entities;
using System;
using System.Linq;
using Utilities.Security;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            try
            {
                // ✅ Apply pending migrations instead of EnsureCreated()
                context.Database.Migrate();

                // ✅ Check if the admin user already exists
                if (!context.Users.Any(u => u.Email == "admin@example.com"))
                {
                    context.Users.Add(new User
                    {
                        FirstName = "Admin",
                        LastName = "User",
                        Email = "admin@example.com",
                        PasswordHash = PasswordHelper.HashPassword("admin123"), // ✅ Hash password correctly
                        Role = "Admin",
                        CreatedAt = DateTime.UtcNow, // ✅ Ensure CreatedAt has a value
                    });

                    context.SaveChanges();
                    Console.WriteLine("✅ Admin user seeded successfully.");
                }
                else
                {
                    Console.WriteLine("⚠️ Admin user already exists. Skipping seeding.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔴 Error seeding admin user: {ex.Message}");
            }
        }
    }
}
