using EcommerceApp.Domain.Entities.Common;
using EcommerceApp.Domain.Entities.Vendor;
using EcommerceApp.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Domain.Entities.Identity;

public class User : BaseEntity
{

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(20)]
    [Phone]
    public string Phone { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; } = UserRole.User;

    public bool IsActive { get; set; } = true;

    public bool IsEmailVerified { get; set; } = false;

    public bool IsPhoneVerified { get; set; } = false;

    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpiry { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }

    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public string? ProfilePictureUrl { get; set; }
    public string? Bio { get; set; }

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public virtual ICollection<UserAddress> Addresses { get; set; } = new List<UserAddress>();
    public virtual global::EcommerceApp.Domain.Entities.Vendor.Vendor? Vendor { get; set; }
}

