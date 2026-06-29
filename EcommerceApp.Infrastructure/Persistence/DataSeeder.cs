using EcommerceApp.Domain.Entities.Identity;
using EcommerceApp.Domain.Entities.Vendor;
using EcommerceApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EcommerceApp.Infrastructure.Persistence;

public static class DataSeeder
{
    // Fixed GUIDs so re-running the seeder never creates duplicates
    private static readonly Guid AdminUserId  = Guid.Parse("11111111-0000-0000-0000-000000000001");
    private static readonly Guid NormalUserId = Guid.Parse("22222222-0000-0000-0000-000000000002");
    private static readonly Guid VendorUserId = Guid.Parse("33333333-0000-0000-0000-000000000003");
    private static readonly Guid VendorId     = Guid.Parse("44444444-0000-0000-0000-000000000004");

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger  = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            // Apply any pending migrations automatically
            await context.Database.MigrateAsync();

            await SeedUsersAsync(context, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    // -------------------------------------------------------------------------
    private static async Task SeedUsersAsync(ApplicationDbContext context, ILogger logger)
    {
        var now = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // --- 1. Admin user -------------------------------------------------------
        if (!await context.Users.IgnoreQueryFilters().AnyAsync(u => u.Id == AdminUserId))
        {
            var admin = new User
            {
                Id                    = AdminUserId,
                FullName              = "System Admin",
                Email                 = "admin@ecommerce.com",
                PasswordHash          = BCrypt.Net.BCrypt.HashPassword("Admin@1234"),
                Phone                 = "0000000001",
                Role                  = UserRole.Admin,
                IsActive              = true,
                IsEmailVerified       = true,
                EmailVerifiedAt       = now,
                CreatedAt             = now,
            };

            await context.Users.AddAsync(admin);
            logger.LogInformation("Seeded admin user: {Email}", admin.Email);
        }

        // --- 2. Normal user ------------------------------------------------------
        if (!await context.Users.IgnoreQueryFilters().AnyAsync(u => u.Id == NormalUserId))
        {
            var normalUser = new User
            {
                Id                    = NormalUserId,
                FullName              = "Normal User",
                Email                 = "user@ecommerce.com",
                PasswordHash          = BCrypt.Net.BCrypt.HashPassword("User@1234"),
                Phone                 = "0000000002",
                Role                  = UserRole.User,
                IsActive              = true,
                IsEmailVerified       = true,
                EmailVerifiedAt       = now,
                CreatedAt             = now,
            };

            await context.Users.AddAsync(normalUser);
            logger.LogInformation("Seeded normal user: {Email}", normalUser.Email);
        }

        // --- 3. Vendor user + Vendor record -------------------------------------
        if (!await context.Users.IgnoreQueryFilters().AnyAsync(u => u.Id == VendorUserId))
        {
            var vendorUser = new User
            {
                Id                    = VendorUserId,
                FullName              = "Seed Vendor",
                Email                 = "vendor@ecommerce.com",
                PasswordHash          = BCrypt.Net.BCrypt.HashPassword("Vendor@1234"),
                Phone                 = "0000000003",
                Role                  = UserRole.Seller,
                IsActive              = true,
                IsEmailVerified       = true,
                EmailVerifiedAt       = now,
                CreatedAt             = now,
            };

            await context.Users.AddAsync(vendorUser);
            logger.LogInformation("Seeded vendor user: {Email}", vendorUser.Email);
        }

        if (!await context.Vendors.IgnoreQueryFilters().AnyAsync(v => v.Id == VendorId))
        {
            var vendor = new Vendor
            {
                Id                 = VendorId,
                UserId             = VendorUserId,
                ShopName           = "Seed Shop",
                ShopSlug           = "seed-shop",
                Description        = "Seeded vendor shop for development.",
                PhoneNumber        = "0000000003",
                Email              = "vendor@ecommerce.com",
                Status             = VendorStatus.Approved,
                IsApproved         = true,
                ApprovedAt         = now,
                CreatedAt          = now,
            };

            await context.Vendors.AddAsync(vendor);
            logger.LogInformation("Seeded vendor record for shop: {ShopName}", vendor.ShopName);
        }

        await context.SaveChangesAsync();
    }
}
