using EcommerceApp.Application.DTOs.Auth;
using EcommerceApp.Application.DTOs.Shared;
using EcommerceApp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommerceApp.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Get detailed profile of the logged-in user (including Addresses and Vendor info)
        /// </summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetCurrentUserId();
            var profile = await _userService.GetUserProfileWithDetailsAsync(userId);

            if (profile == null)
                return NotFound(ApiResponse<object>.Fail("User profile not found"));

            return Ok(ApiResponse<object>.Ok(profile, "Profile retrieved successfully"));
        }

        /// <summary>
        /// Update profile details of the logged-in user
        /// </summary>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var success = await _userService.UpdateProfileAsync(userId, request);

            if (!success)
                return BadRequest(ApiResponse<object>.Fail("Failed to update profile. User may not exist."));

            return Ok(ApiResponse<object>.Ok(null, "Profile updated successfully"));
        }

        /// <summary>
        /// Change password of the logged-in user
        /// </summary>
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var success = await _userService.ChangePasswordAsync(userId, request);

            if (!success)
                return BadRequest(ApiResponse<object>.Fail("Failed to change password. Please verify your current password is correct."));

            return Ok(ApiResponse<object>.Ok(null, "Password changed successfully. Active sessions on other devices have been logged out."));
        }

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirst("userId")?.Value;
            if (!Guid.TryParse(userId, out var parsedUserId))
            {
                throw new UnauthorizedAccessException("User context is invalid or missing");
            }
            return parsedUserId;
        }
    }
}
