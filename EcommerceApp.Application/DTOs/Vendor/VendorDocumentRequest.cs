using EcommerceApp.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Application.DTOs.Vendor;

public class VendorDocumentRequest
{
    [Required]
    public DocumentType DocumentType { get; set; }

    [Required]
    public IFormFile File { get; set; } = null!;
}
