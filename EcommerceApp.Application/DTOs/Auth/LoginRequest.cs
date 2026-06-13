using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Application.DTOs.Auth
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(150, ErrorMessage = "Email cannot exceed 150 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Two-factor authentication code (if enabled)
        /// </summary>
        public string? TwoFactorCode { get; set; }

        /// <summary>
        /// Remember me for 30 days
        /// </summary>
        public bool RememberMe { get; set; } = false;
    }
}