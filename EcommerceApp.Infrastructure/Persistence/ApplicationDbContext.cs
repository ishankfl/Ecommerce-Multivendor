using Ecommerce.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace EcommerceApp.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<UserAddress> UserAddresses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();

            entity.Property(x => x.FullName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Email)
                .HasMaxLength(150)
                .IsRequired();

            entity.HasIndex(x => x.Email).IsUnique();

            entity.Property(x => x.PasswordHash)
                .HasColumnName("password_hash")
                .IsRequired();

            entity.Property(x => x.Phone)
                .HasMaxLength(20);

            entity.Property(x => x.Role)
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("User");

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.IsEmailVerified)
                .HasDefaultValue(false);

            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            entity.HasMany(x => x.RefreshTokens)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Addresses)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RefreshToken Configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();

            entity.Property(x => x.Token)
                .IsRequired()
                .HasMaxLength(500);

            entity.HasIndex(x => x.Token).IsUnique();

            entity.Property(x => x.ExpiryDate)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // UserAddress Configuration
        modelBuilder.Entity<UserAddress>(entity =>
        {
            entity.ToTable("user_addresses");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();

            entity.Property(x => x.AddressLine1)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(x => x.City)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Country)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.PostalCode)
                .IsRequired()
                .HasMaxLength(20);
        });
    }
}