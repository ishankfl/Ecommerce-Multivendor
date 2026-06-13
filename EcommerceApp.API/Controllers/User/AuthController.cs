using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using EcommerceApp.Application.DTOs.Auth;
using EcommerceApp.Application.Interfaces.Services;

namespace EcommerceApp.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [EnableRateLimiting("fixed")]
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
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.RegisterAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Login user
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.LoginAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Verify email address with token
        /// </summary>
        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string email, [FromQuery] string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
                return BadRequest(new { message = "Email and token are required" });

            var result = await _userService.VerifyEmailAsync(email, token);

            if (result)
            {
                return Ok(new
                {
                    success = true,
                    message = "Email verified successfully. You can now login."
                });
            }

            return BadRequest(new
            {
                success = false,
                message = "Invalid or expired verification link. Please request a new verification email."
            });
        }

        /// <summary>
        /// Resend verification email
        /// </summary>
        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.ResendVerificationEmailAsync(request.Email);

            if (result)
            {
                return Ok(new
                {
                    success = true,
                    message = "Verification email sent. Please check your inbox."
                });
            }

            return BadRequest(new
            {
                success = false,
                message = "Unable to send verification email. Email might already be verified or doesn't exist."
            });
        }

        /// <summary>
        /// Request password reset
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _userService.ForgotPasswordAsync(request.Email);

            // Always return success to prevent email enumeration
            return Ok(new
            {
                success = true,
                message = "If your email is registered and verified, you will receive a password reset link."
            });
        }

        /// <summary>
        /// Reset password with token
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.ResetPasswordAsync(request);

            if (result)
            {
                return Ok(new
                {
                    success = true,
                    message = "Password reset successful. You can now login with your new password."
                });
            }

            return BadRequest(new
            {
                success = false,
                message = "Invalid or expired reset token. Please request a new password reset."
            });
        }

        /// <summary>
        /// Refresh JWT token
        /// </summary>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest(new { message = "Refresh token is required" });

            try
            {
                var result = await _userService.RefreshTokenAsync(refreshToken);

                if (!result.Success)
                    return Unauthorized(result);

                return Ok(result);
            }
            catch (Exception)
            {
                return Unauthorized(new { message = "Invalid refresh token" });
            }
        }

        /// <summary>
        /// Get current user profile (Protected)
        /// </summary>
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst("userId")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not found" });

            var user = await _userService.GetUserByIdAsync(Guid.Parse(userId));

            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.Phone,
                user.IsEmailVerified,
                user.IsActive,
                user.Role,
                user.CreatedAt,
                user.LastLoginAt
            });
        }

        /// <summary>
        /// Logout user (Protected)
        /// </summary>
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirst("userId")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                await _userService.LogoutAsync(Guid.Parse(userId));
            }

            return Ok(new { message = "Logged out successfully" });
        }

        /// <summary>
        /// Check if user is authenticated (Protected)
        /// </summary>
        [Authorize]
        [HttpGet("check-auth")]
        public IActionResult CheckAuth()
        {
            var userId = User.FindFirst("userId")?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var name = User.FindFirst(ClaimTypes.Name)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var isEmailVerified = User.FindFirst("isEmailVerified")?.Value;

            return Ok(new
            {
                isAuthenticated = true,
                userId,
                email,
                name,
                role,
                isEmailVerified = bool.Parse(isEmailVerified ?? "false")
            });
        }
    }
}