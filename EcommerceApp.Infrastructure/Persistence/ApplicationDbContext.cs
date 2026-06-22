using EcommerceApp.Domain.Entities.Identity;
using EcommerceApp.Domain.Entities.Vendor;
using EcommerceApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<UserAddress> UserAddresses { get; set; } = null!;
    public DbSet<LoginHistory> LoginHistories { get; set; } = null!;
    public DbSet<Vendor> Vendors { get; set; } = null!;
    public DbSet<VendorDocument> VendorDocuments { get; set; } = null!;
    public DbSet<VendorBankAccount> VendorBankAccounts { get; set; } = null!;
    public DbSet<VendorAddress> VendorAddresses { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnType("char(36)");
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Role).HasConversion<int>().HasDefaultValue(UserRole.User);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsEmailVerified).HasDefaultValue(false);
            entity.Property(e => e.IsPhoneVerified).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ProfilePictureUrl).HasMaxLength(500);
            entity.Property(e => e.Bio).HasMaxLength(500);

            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.DeletedAt);
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.HasMany(e => e.RefreshTokens).WithOne(e => e.User).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Addresses).WithOne(e => e.User).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Vendor).WithOne(e => e.User).HasForeignKey<Vendor>(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnType("char(36)").ValueGeneratedOnAdd();
            entity.Property(e => e.Token).IsRequired().HasMaxLength(500);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.Property(e => e.ExpiryDate).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.CreatedByIp).HasMaxLength(50);
            entity.Property(e => e.RevokedByIp).HasMaxLength(50);
            entity.Property(e => e.ReplacedByToken).HasMaxLength(500);
            entity.HasIndex(e => new { e.UserId, e.IsRevoked });
            entity.HasIndex(e => e.ExpiryDate);
        });

        modelBuilder.Entity<UserAddress>(entity =>
        {
            entity.ToTable("UserAddresses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnType("char(36)").ValueGeneratedOnAdd();
            entity.Property(e => e.AddressLine1).IsRequired().HasMaxLength(255);
            entity.Property(e => e.City).IsRequired().HasMaxLength(100);
            entity.Property(e => e.State).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Country).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PostalCode).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Landmark).HasMaxLength(50);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.RecipientName).HasMaxLength(100);
            entity.Property(e => e.AddressType).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(e => new { e.UserId, e.IsDefault });
        });

        modelBuilder.Entity<LoginHistory>(entity =>
        {
            entity.ToTable("LoginHistories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnType("char(36)").ValueGeneratedOnAdd();
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.UserAgent).HasMaxLength(200);
            entity.Property(e => e.FailureReason).HasMaxLength(500);
            entity.Property(e => e.LoginTime).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(e => new { e.UserId, e.LoginTime });
            entity.HasIndex(e => e.LoginTime);
        });

        modelBuilder.Entity<Vendor>(entity =>
        {
            entity.ToTable("Vendors");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnType("char(36)").ValueGeneratedOnAdd();
            entity.Property(e => e.UserId).HasColumnType("char(36)");
            entity.Property(e => e.ShopName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ShopSlug).IsRequired().HasMaxLength(120);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.CoverImageUrl).HasMaxLength(500);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.Status).HasConversion<int>().HasDefaultValue(VendorStatus.Pending);
            entity.Property(e => e.PANNumber).HasMaxLength(50);
            entity.Property(e => e.VATNumber).HasMaxLength(50);
            entity.Property(e => e.RegistrationNumber).HasMaxLength(50);
            entity.Property(e => e.RejectionReason).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasIndex(e => e.ShopName).IsUnique();
            entity.HasIndex(e => e.ShopSlug).IsUnique();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.DeletedAt);
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.HasIndex(e => e.Status);
            entity.HasMany(e => e.Documents).WithOne(e => e.Vendor).HasForeignKey(e => e.VendorId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<VendorDocument>(entity =>
        {
            entity.ToTable("VendorDocuments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnType("char(36)").ValueGeneratedOnAdd();
            entity.Property(e => e.VendorId).HasColumnType("char(36)");
            entity.Property(e => e.DocumentType).HasConversion<int>();
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FileUrl).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FileExtension).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.DeletedAt);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => new { e.VendorId, e.DocumentType });
        });

        modelBuilder.Entity<VendorBankAccount>(entity =>
        {
            entity.ToTable("VendorBankAccounts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnType("char(36)").ValueGeneratedOnAdd();
            entity.Property(e => e.BankName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.AccountHolderName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.AccountNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.BranchName).HasMaxLength(100);
            entity.HasOne(e => e.Vendor).WithOne(e => e.BankAccount).HasForeignKey<VendorBankAccount>(e => e.VendorId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<VendorAddress>(entity =>
        {
            entity.ToTable("VendorAddresses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnType("char(36)").ValueGeneratedOnAdd();
            entity.Property(e => e.AddressLine1).IsRequired().HasMaxLength(255);
            entity.Property(e => e.AddressLine2).HasMaxLength(255);
            entity.Property(e => e.City).IsRequired().HasMaxLength(100);
            entity.Property(e => e.State).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Country).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PostalCode).IsRequired().HasMaxLength(20);
            entity.HasOne(e => e.Vendor).WithOne(e => e.Address).HasForeignKey<VendorAddress>(e => e.VendorId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
