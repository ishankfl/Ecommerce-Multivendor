using EcommerceApp.Domain.Enums;

namespace EcommerceApp.Application.DTOs.Vendor;

public class VendorResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string ShopSlug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public VendorStatus Status { get; set; }
    public bool IsApproved { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<VendorDocumentResponse> Documents { get; set; } = new();
}
