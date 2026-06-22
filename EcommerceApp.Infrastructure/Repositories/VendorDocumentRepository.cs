using EcommerceApp.Application.Interfaces.Repositories;
using EcommerceApp.Domain.Entities.Vendor;
using EcommerceApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Infrastructure.Repositories;

public class VendorDocumentRepository : GenericRepository<VendorDocument>, IVendorDocumentRepository
{
    public VendorDocumentRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public Task<VendorDocument?> GetDocumentForVendorAsync(Guid vendorId, Guid documentId) =>
        DbContext.VendorDocuments.FirstOrDefaultAsync(d => d.VendorId == vendorId && d.Id == documentId);

    public async Task<IReadOnlyList<VendorDocument>> GetDocumentsForVendorAsync(Guid vendorId) =>
        await DbContext.VendorDocuments
            .AsNoTracking()
            .Where(d => d.VendorId == vendorId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
}
