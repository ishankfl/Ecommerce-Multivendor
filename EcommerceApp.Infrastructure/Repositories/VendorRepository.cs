using EcommerceApp.Application.Interfaces.Repositories;
using EcommerceApp.Domain.Enums;
using EcommerceApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using VendorEntity = EcommerceApp.Domain.Entities.Vendor.Vendor;

namespace EcommerceApp.Infrastructure.Repositories;

public class VendorRepository : GenericRepository<VendorEntity>, IVendorRepository
{
    public VendorRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public Task<VendorEntity?> GetVendorByUserIdAsync(Guid userId) =>
        DbContext.Vendors
            .Include(v => v.Documents)
            .Include(v => v.User)
            .FirstOrDefaultAsync(v => v.UserId == userId);

    public Task<VendorEntity?> GetVendorWithDocumentsAsync(Guid vendorId) =>
        DbContext.Vendors
            .Include(v => v.Documents)
            .Include(v => v.User)
            .FirstOrDefaultAsync(v => v.Id == vendorId);

    public Task<VendorEntity?> GetVendorWithDocumentsByUserIdAsync(Guid userId) =>
        DbContext.Vendors
            .Include(v => v.Documents)
            .Include(v => v.User)
            .FirstOrDefaultAsync(v => v.UserId == userId);

    public async Task<IReadOnlyList<VendorEntity>> GetAllVendorsAsync() =>
        await DbContext.Vendors
            .AsNoTracking()
            .Include(v => v.Documents)
            .Include(v => v.User)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();

    public async Task<IReadOnlyList<VendorEntity>> GetPendingVendorsAsync() =>
        await DbContext.Vendors
            .AsNoTracking()
            .Include(v => v.Documents)
            .Include(v => v.User)
            .Where(v => v.Status == VendorStatus.Pending || v.Status == VendorStatus.UnderReview)
            .OrderBy(v => v.CreatedAt)
            .ToListAsync();

    public async Task<IReadOnlyList<VendorEntity>> GetApprovedVendorsAsync() =>
        await DbContext.Vendors
            .AsNoTracking()
            .Include(v => v.Documents)
            .Where(v => v.Status == VendorStatus.Approved && v.IsApproved)
            .OrderBy(v => v.ShopName)
            .ToListAsync();

    public async Task<IReadOnlyList<VendorEntity>> GetRejectedVendorsAsync() =>
        await DbContext.Vendors
            .AsNoTracking()
            .Include(v => v.Documents)
            .Include(v => v.User)
            .Where(v => v.Status == VendorStatus.Rejected)
            .OrderByDescending(v => v.UpdatedAt ?? v.CreatedAt)
            .ToListAsync();

    public Task<int> CountByStatusAsync(VendorStatus status) =>
        DbContext.Vendors.CountAsync(v => v.Status == status);

    public Task<bool> IsShopNameUniqueAsync(string shopName, Guid? excludeVendorId = null) =>
        DbContext.Vendors.AllAsync(v => v.ShopName != shopName || (excludeVendorId.HasValue && v.Id == excludeVendorId.Value));

    public Task<bool> IsShopSlugUniqueAsync(string shopSlug, Guid? excludeVendorId = null) =>
        DbContext.Vendors.AllAsync(v => v.ShopSlug != shopSlug || (excludeVendorId.HasValue && v.Id == excludeVendorId.Value));
}
