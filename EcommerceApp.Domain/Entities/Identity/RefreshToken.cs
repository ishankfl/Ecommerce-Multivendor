using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApp.Domain.Entities.Identity;

public class RefreshToken
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(500)]
    public string Token { get; set; } = string.Empty;

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public DateTime ExpiryDate { get; set; }

    public bool IsRevoked { get; set; } = false;

    public bool IsUsed { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? CreatedByIp { get; set; }

    public DateTime? RevokedAt { get; set; }

    public string? RevokedByIp { get; set; }

    public string? ReplacedByToken { get; set; }

    // Navigation property
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}