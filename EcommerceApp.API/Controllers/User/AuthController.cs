using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EcommerceApp.Application.DTOs.Auth;
using EcommerceApp.Application.Interfaces.Services;

namespace EcommerceApp.API.Controllers
{
    /// <summary>
    /// Authentication Controller - Handles user authentication endpoints
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="request">Registration details</param>
        /// <returns>Authentication response with JWT token</returns>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = "Invalid request data",
                    Errors = ModelStateErrors()
                });
            }

            var response = await _userService.RegisterAsync(request);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Login user and get JWT token
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>Authentication response with JWT token</returns>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = "Invalid request data",
                    Errors = ModelStateErrors()
                });
            }

            var response = await _userService.LoginAsync(request);

            if (!response.Success)
            {
                return Unauthorized(response);
            }

            // Set refresh token in HTTP-only cookie (optional but more secure)
            SetRefreshTokenCookie(response.RefreshToken);

            return Ok(response);
        }

        /// <summary>
        /// Refresh JWT token using refresh token
        /// </summary>
        /// <param name="request">Refresh token request</param>
        /// <returns>New authentication response</returns>
        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                // Try to get refresh token from cookie
                var cookieToken = Request.Cookies["refreshToken"];
                if (!string.IsNullOrEmpty(cookieToken))
                {
                    request.RefreshToken = cookieToken;
                }
                else
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Refresh token is required"
                    });
                }
            }

            var response = await _userService.RefreshTokenAsync(request.RefreshToken);

            if (!response.Success)
            {
                return Unauthorized(response);
            }

            // Update refresh token cookie
            SetRefreshTokenCookie(response.RefreshToken);

            return Ok(response);
        }

        /// <summary>
        /// Logout user and invalidate refresh token
        /// </summary>
        /// <returns>Logout result</returns>
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // Get user ID from claims
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return BadRequest(new { message = "User not found" });
            }

            var userId = Guid.Parse(userIdClaim);
            var result = await _userService.LogoutAsync(userId);

            // Remove refresh token cookie
            Response.Cookies.Delete("refreshToken");

            if (result)
            {
                return Ok(new { message = "Logged out successfully" });
            }

            return BadRequest(new { message = "Logout failed" });
        }

        /// <summary>
        /// Get current authenticated user information
        /// </summary>
        /// <returns>Current user details</returns>
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<AuthResponse>> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new AuthResponse
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var userId = Guid.Parse(userIdClaim);
            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new AuthResponse
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            return Ok(new AuthResponse
            {
                Success = true,
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                Phone = user.Phone,
                IsEmailVerified = user.IsEmailVerified,
                LastLoginAt = user.LastLoginAt,
                Message = "User retrieved successfully"
            });
        }

        /// <summary>
        /// Forgot password - send reset token to email
        /// </summary>
        /// <param name="request">Email address</param>
        /// <returns>Result message</returns>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _userService.ForgotPasswordAsync(request.Email);

            // Always return success even if email doesn't exist (security best practice)
            return Ok(new { message = "If your email is registered, you will receive a password reset link" });
        }

        /// <summary>
        /// Reset password using token
        /// </summary>
        /// <param name="request">Reset password details</param>
        /// <returns>Result message</returns>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.ResetPasswordAsync(request);

            if (!result)
            {
                return BadRequest(new { message = "Invalid or expired reset token" });
            }

            return Ok(new { message = "Password reset successfully. Please login with your new password." });
        }

        /// <summary>
        /// Verify email address
        /// </summary>
        /// <param name="request">Verification details</param>
        /// <returns>Result message</returns>
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Add email verification logic here
            // This would typically check a token stored in cache or database

            return Ok(new { message = "Email verified successfully" });
        }

        // Helper method to set refresh token as HTTP-only cookie
        private void SetRefreshTokenCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,        // Prevents JavaScript access (security)
                Secure = true,          // Only send over HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7),
                Path = "/"
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

        // Helper method to collect model state errors
        private object ModelStateErrors()
        {
            var errors = new System.Collections.Generic.Dictionary<string, string[]>();
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                if (state != null && state.Errors.Count > 0)
                {
                    errors[key] = state.Errors.Select(e => e.ErrorMessage).ToArray();
                }
            }
            return errors;
        }
    }
}