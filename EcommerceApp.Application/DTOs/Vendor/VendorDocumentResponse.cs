using EcommerceApp.Domain.Enums;

namespace EcommerceApp.Application.DTOs.Vendor;

public class VendorDocumentResponse
{
    public Guid Id { get; set; }
    public DocumentType DocumentType { get; set; }
    public string DocumentTypeName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public bool IsVerified { get; set; }
    public DateTime UploadedAt { get; set; }
}
