using EcommerceApp.Application.DTOs.Auth;
using EcommerceApp.Domain.Entities.Identity;

namespace EcommerceApp.Application.Interfaces.Services
{
    public interface IUserService
    {
        // 
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);

        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(ResetPasswordRequest request);

        Task<User?> GetUserByIdAsync(Guid id);
        Task<User?> GetUserByEmailAsync(string email);

        Task<AuthResponse> RefreshTokenAsync(string refreshToken);

        Task<bool> LogoutAsync(Guid userId);
    }
}