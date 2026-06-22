using EcommerceApp.Application.DTOs.Vendor;

namespace EcommerceApp.Application.Interfaces.Services;

public interface IVendorService
{
    Task<VendorResponse> RegisterVendorAsync(VendorRegisterRequest request);
    Task<VendorResponse> ApplyVendorAsync(Guid userId, VendorApplicationRequest request);
    Task<VendorResponse> UploadDocumentAsync(Guid userId, VendorDocumentRequest request);
    Task<VendorDocumentResponse> GetDocumentAsync(Guid userId, Guid documentId);
    Task<IReadOnlyList<VendorDocumentResponse>> GetDocumentsAsync(Guid userId);
    Task<bool> DeleteDocumentAsync(Guid userId, Guid documentId);
    Task<VendorResponse> GetVendorProfileAsync(Guid userId);
    Task<bool> UpdateVendorProfileAsync(Guid userId, UpdateVendorRequest request);
    Task<IReadOnlyList<VendorResponse>> GetPendingVendorsAsync();
    Task<VendorResponse> ApproveVendorAsync(Guid adminId, VendorApprovalRequest request);
    Task<VendorResponse> RejectVendorAsync(Guid adminId, VendorRejectionRequest request);
}

