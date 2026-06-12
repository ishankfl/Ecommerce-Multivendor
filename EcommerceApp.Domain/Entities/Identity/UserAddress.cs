using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApp.Domain.Entities.Identity;

public class UserAddress
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(255)]
    public string AddressLine1 { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? AddressLine2 { get; set; }

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string State { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Country { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string PostalCode { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Landmark { get; set; }

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [MaxLength(100)]
    public string? RecipientName { get; set; }

    public bool IsDefault { get; set; } = false;

    public string? AddressType { get; set; } // "Home", "Work", "Other"

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
}