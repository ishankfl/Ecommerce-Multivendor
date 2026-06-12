using EcommerceApp.Domain.Entities.Identity;
using EcommerceApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    //public ApplicationDbContext()
    //{
    //}

    // DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<UserAddress> UserAddresses { get; set; }
    public DbSet<LoginHistory> LoginHistories { get; set; }

    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //{
    //    // ✅ ADD THIS - Configure connection for design-time when no options are provided
    //    if (!optionsBuilder.IsConfigured)
    //    {
    //        var connectionString = "server=localhost;port=3306;database=ecommerce_db;user=root;password=";
    //        optionsBuilder.UseMySql(
    //            connectionString,
    //            ServerVersion.AutoDetect(connectionString)
    //        );
    //    }
    //}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(150);

            entity.HasIndex(e => e.Email)
                .IsUnique();

            entity.Property(e => e.PasswordHash)
                .IsRequired();

            entity.Property(e => e.Phone)
                .HasMaxLength(20);

            entity.Property(e => e.Role)
                .HasConversion<int>()
                .HasDefaultValue(UserRole.User);

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.Property(e => e.IsEmailVerified)
                .HasDefaultValue(false);

            entity.Property(e => e.IsPhoneVerified)
                .HasDefaultValue(false);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.ProfilePictureUrl)
                .HasMaxLength(500);

            entity.Property(e => e.Bio)
                .HasMaxLength(500);

            // Relationships
            entity.HasMany(e => e.RefreshTokens)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Addresses)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RefreshToken Configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Token)
                .IsRequired()
                .HasMaxLength(500);

            entity.HasIndex(e => e.Token)
                .IsUnique();

            entity.Property(e => e.ExpiryDate)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.CreatedByIp)
                .HasMaxLength(50);

            entity.Property(e => e.RevokedByIp)
                .HasMaxLength(50);

            entity.Property(e => e.ReplacedByToken)
                .HasMaxLength(500);

            // Index for performance
            entity.HasIndex(e => new { e.UserId, e.IsRevoked });
            entity.HasIndex(e => e.ExpiryDate);
        });

        // UserAddress Configuration
        modelBuilder.Entity<UserAddress>(entity =>
        {
            entity.ToTable("UserAddresses");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.AddressLine1)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.City)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.State)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Country)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.PostalCode)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.Landmark)
                .HasMaxLength(50);

            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20);

            entity.Property(e => e.RecipientName)
                .HasMaxLength(100);

            entity.Property(e => e.AddressType)
                .HasMaxLength(20);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Only one default address per user
            entity.HasIndex(e => new { e.UserId, e.IsDefault })
                .IsUnique()
                .HasFilter("IsDefault = 1");
        });

        // LoginHistory Configuration
        modelBuilder.Entity<LoginHistory>(entity =>
        {
            entity.ToTable("LoginHistories");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.IpAddress)
                .HasMaxLength(50);

            entity.Property(e => e.UserAgent)
                .HasMaxLength(200);

            entity.Property(e => e.FailureReason)
                .HasMaxLength(500);

            entity.Property(e => e.LoginTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Index for faster queries
            entity.HasIndex(e => new { e.UserId, e.LoginTime });
            entity.HasIndex(e => e.LoginTime);
        });

        // Seed data for admin user
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Create default admin user (password: Admin@123)
        // In real app, hash this password properly
        var adminUser = new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            FullName = "System Administrator",
            Email = "admin@ecommerce.com",
            PasswordHash = "$2a$11$K8gJqX7XpK8gJqX7XpK8uO3Q5R6sT7uV8wX9yZ0aB1cD2eF3gH4iJ5kL6mN7oP8", // "Admin@123" hashed
            Phone = "1234567890",
            Role = UserRole.Admin,
            IsActive = true,
            IsEmailVerified = true,
            IsPhoneVerified = true,
            CreatedAt = DateTime.UtcNow
        };

        modelBuilder.Entity<User>().HasData(adminUser);
    }
}