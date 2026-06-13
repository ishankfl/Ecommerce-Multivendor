using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Application.DTOs.Auth
{
    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "Refresh token is required")]
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Optional: Current access token (for validation)
        /// </summary>
        public string? AccessToken { get; set; }
    }
}