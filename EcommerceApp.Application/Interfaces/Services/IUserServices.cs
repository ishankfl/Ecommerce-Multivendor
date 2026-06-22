using EcommerceApp.Application.DTOs.Auth;
using EcommerceApp.Domain.Entities.Identity;

namespace EcommerceApp.Application.Interfaces.Services
{
    public interface IUserService
    {
        // Authentication
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);

        // Password Management
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(ResetPasswordRequest request);

        // Email Verification (NEW)
        Task<bool> VerifyEmailAsync(string email, string token);
        Task<bool> ResendVerificationEmailAsync(string email);

        // User Queries
        Task<User?> GetUserByIdAsync(Guid id);
        Task<User?> GetUserByEmailAsync(string email);

        // Token Management
        Task<AuthResponse> RefreshTokenAsync(string refreshToken);
        Task<bool> LogoutAsync(Guid userId);

        // Profile & Password Management
        Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
        Task<bool> UpdateProfileAsync(Guid userId, UpdateUserProfileRequest request);
        Task<object?> GetUserProfileWithDetailsAsync(Guid userId);
    }
}