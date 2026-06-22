using EcommerceApp.Domain.Entities.Common;
using EcommerceApp.Domain.Entities.Identity;
using EcommerceApp.Domain.Enums;

namespace EcommerceApp.Domain.Entities.Vendor;

public class Vendor : BaseEntity
{
    public Guid UserId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string ShopSlug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public VendorStatus Status { get; set; } = VendorStatus.Pending;
    public bool IsApproved { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? PANNumber { get; set; }
    public string? VATNumber { get; set; }
    public string? RegistrationNumber { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual ICollection<VendorDocument> Documents { get; set; } = new List<VendorDocument>();
    public virtual VendorBankAccount? BankAccount { get; set; }
    public virtual VendorAddress? Address { get; set; }
}
