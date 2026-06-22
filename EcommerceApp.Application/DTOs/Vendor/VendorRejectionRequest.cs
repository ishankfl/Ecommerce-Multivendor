using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Application.DTOs.Vendor;

public class VendorRejectionRequest
{
    [Required]
    public Guid VendorId { get; set; }

    [Required, MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}
