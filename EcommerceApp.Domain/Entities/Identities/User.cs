using EcommerceApp.Domain.Entities.Identities;
using EcommerceApp.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Domain.Entities.Identity;

public class User
{
    [Key]
    public int Id { get; set; }

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

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public string? ProfilePictureUrl { get; set; }

    public string? Bio { get; set; }

    // Navigation properties
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public virtual ICollection<UserAddress> Addresses { get; set; } = new List<UserAddress>();
}