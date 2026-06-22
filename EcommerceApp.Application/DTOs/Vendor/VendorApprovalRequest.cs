using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Application.DTOs.Vendor;

public class VendorApprovalRequest
{
    [Required]
    public Guid VendorId { get; set; }
}
