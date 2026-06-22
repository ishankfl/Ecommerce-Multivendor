using EcommerceApp.Domain.Entities.Common;
using EcommerceApp.Domain.Enums;

namespace EcommerceApp.Domain.Entities.Vendor;

public class VendorDocument : BaseEntity
{
    public Guid VendorId { get; set; }
    public DocumentType DocumentType { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileExtension { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public Guid? VerifiedBy { get; set; }

    public virtual Vendor Vendor { get; set; } = null!;
}
