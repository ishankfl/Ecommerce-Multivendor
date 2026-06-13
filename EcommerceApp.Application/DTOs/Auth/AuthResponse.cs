using System;
using System.Collections.Generic;

namespace EcommerceApp.Application.DTOs.Auth
{
    /// <summary>
    /// Authentication response DTO - Returned after successful login/registration
    /// </summary>
    public class AuthResponse
    {
        /// <summary>
        /// Indicates if the authentication was successful
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Message describing the result (success or error)
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Collection of validation errors (if any)
        /// </summary>
        public object? Errors { get; set; }

        /// <summary>
        /// Error code for programmatic handling
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// User's unique identifier
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// User's full name
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// User's email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User's role (Admin, User, Seller)
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// JWT access token
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Refresh token for obtaining new access tokens
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Token expiration time (UTC)
        /// </summary>
        public DateTime TokenExpiry { get; set; }

        /// <summary>
        /// User's phone number (optional)
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Indicates if email is verified
        /// </summary>
        public bool IsEmailVerified { get; set; }

        /// <summary>
        /// User's last login time
        /// </summary>
        public DateTime? LastLoginAt { get; set; }
    }
}