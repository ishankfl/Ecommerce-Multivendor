using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Application.DTOs.Vendor;

public class UpdateVendorRequest
{
    [Required, MinLength(3), MaxLength(100)]
    public string ShopName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Phone, MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    [MaxLength(50)]
    public string? PANNumber { get; set; }

    [MaxLength(50)]
    public string? VATNumber { get; set; }

    [MaxLength(50)]
    public string? RegistrationNumber { get; set; }
}
