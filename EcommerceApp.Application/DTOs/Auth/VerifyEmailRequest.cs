using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Application.DTOs.Auth
{
    public class VerifyEmailRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Verification token is required")]
        public string Token { get; set; } = string.Empty;
    }
}