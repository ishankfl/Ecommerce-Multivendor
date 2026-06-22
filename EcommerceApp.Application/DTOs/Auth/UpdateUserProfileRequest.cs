using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Application.DTOs.Auth
{
    public class UpdateUserProfileRequest
    {
        [Required, MinLength(2), MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Phone, MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(500)]
        public string? Bio { get; set; }

        [MaxLength(500)]
        public string? ProfilePictureUrl { get; set; }
    }
}
