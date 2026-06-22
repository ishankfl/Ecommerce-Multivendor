using EcommerceApp.Domain.Entities.Common;

namespace EcommerceApp.Domain.Entities.Vendor;

public class VendorBankAccount : BaseEntity
{
    public Guid VendorId { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string AccountHolderName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string? BranchName { get; set; }
    public bool IsVerified { get; set; }

    public virtual Vendor Vendor { get; set; } = null!;
}
