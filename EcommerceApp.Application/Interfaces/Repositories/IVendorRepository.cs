using VendorEntity = EcommerceApp.Domain.Entities.Vendor.Vendor;

namespace EcommerceApp.Application.Interfaces.Repositories;

public interface IVendorRepository : IGenericRepository<VendorEntity>
{
    Task<VendorEntity?> GetVendorByUserIdAsync(Guid userId);
    Task<VendorEntity?> GetVendorWithDocumentsAsync(Guid vendorId);
    Task<VendorEntity?> GetVendorWithDocumentsByUserIdAsync(Guid userId);
    Task<IReadOnlyList<VendorEntity>> GetPendingVendorsAsync();
    Task<IReadOnlyList<VendorEntity>> GetApprovedVendorsAsync();
    Task<bool> IsShopNameUniqueAsync(string shopName, Guid? excludeVendorId = null);
    Task<bool> IsShopSlugUniqueAsync(string shopSlug, Guid? excludeVendorId = null);
}
