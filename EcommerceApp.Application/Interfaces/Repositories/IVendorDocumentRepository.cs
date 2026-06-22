using EcommerceApp.Domain.Entities.Vendor;

namespace EcommerceApp.Application.Interfaces.Repositories;

public interface IVendorDocumentRepository : IGenericRepository<VendorDocument>
{
    Task<VendorDocument?> GetDocumentForVendorAsync(Guid vendorId, Guid documentId);
    Task<IReadOnlyList<VendorDocument>> GetDocumentsForVendorAsync(Guid vendorId);
}
