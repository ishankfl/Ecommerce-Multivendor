using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApp.Domain.Entities.Identity;

public class LoginHistory
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    public DateTime LoginTime { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    [MaxLength(200)]
    public string? UserAgent { get; set; }

    public bool IsSuccessful { get; set; } = true;

    [MaxLength(500)]
    public string? FailureReason { get; set; }

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
}