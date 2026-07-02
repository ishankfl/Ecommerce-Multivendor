using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EcommerceApp.Application.DTOs.Vendor;

public class VendorRejectionRequest
{
    [Required]
    public Guid VendorId { get; set; }

    /// <summary>
    /// Rejection reason. Accepted as "reason" or "rejectionReason" from the frontend.
    /// </summary>
    [MaxLength(500)]
    public string? Reason { get; set; }

    /// <summary>Frontend alias — maps "rejectionReason" JSON key to Reason.</summary>
    [JsonPropertyName("rejectionReason")]
    [MaxLength(500)]
    public string? RejectionReason
    {
        get => Reason;
        set { if (!string.IsNullOrWhiteSpace(value)) Reason = value; }
    }

    /// <summary>Resolved reason with fallback.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public string ResolvedReason => Reason ?? string.Empty;
}
