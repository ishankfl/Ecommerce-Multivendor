using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using EcommerceApp.Application.DTOs.Auth;
using EcommerceApp.Application.Interfaces.Repositories;
using EcommerceApp.Application.Interfaces.Services;
using EcommerceApp.Domain.Entities.Identity;
using EcommerceApp.Domain.Enums;
using BCrypt.Net;

namespace EcommerceApp.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;
        private readonly IEmailService _emailService;

        public UserService(
            IUserRepository userRepo,
            IConfiguration config,
            IMemoryCache cache,
            IEmailService emailService)
        {
            _userRepo = userRepo;
            _config = config;
            _cache = cache;
            _emailService = emailService;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            // Check if email exists
            if (await _userRepo.ExistsByEmailAsync(request.Email))
                return new AuthResponse
                {
                    Message = "Email already exists",
                    Success = false
                };

            // Generate email verification token
            var verificationToken = GenerateRandomToken();

            // Create new user
            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = request.FullName,
                Email = request.Email.ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Phone = request.Phone,
                Role = UserRole.User,
                IsActive = true,
                IsEmailVerified = false,
                EmailVerificationToken = verificationToken,
                EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24),
                CreatedAt = DateTime.UtcNow
            };

            await _userRepo.AddAsync(user);
            await _userRepo.SaveChangesAsync();

            // Send welcome email and verification email (fire and forget)
            var verificationLink = EmailLinkBuilder.BuildVerificationLink(
                _config["AppUrl"],
                user.Email,
                verificationToken);

            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName);
                    await _emailService.SendVerificationEmailAsync(user.Email, user.FullName, verificationLink);
                }
                catch (Exception ex)
                {
                    // Log error but don't break registration
                    Console.WriteLine($"Failed to send email: {ex.Message}");
                }
            });

            return new AuthResponse
            {
                Success = true,
                Message = "Registration successful! Please check your email to verify your account.",
                Email = user.Email,
                FullName = user.FullName,
                RequiresEmailVerification = true,
                IsEmailVerified = false
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var cacheKey = $"login_attempt_{request.Email}";

            // Check login attempts
            if (_cache.TryGetValue(cacheKey, out int attempts) && attempts >= 5)
                return new AuthResponse
                {
                    Message = "Too many failed attempts. Please try again later.",
                    Success = false
                };

            // Get user by email
            var user = await _userRepo.GetByEmailAsync(request.Email);

            // Verify password
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                // Increment failed attempts
                _cache.Set(cacheKey, (attempts + 1), TimeSpan.FromMinutes(15));
                return new AuthResponse
                {
                    Message = "Invalid email or password",
                    Success = false
                };
            }

            // Check if account is active
            if (!user.IsActive)
            {
                return new AuthResponse
                {
                    Message = "Account is deactivated. Please contact support.",
                    Success = false
                };
            }

            // Check if email is verified
            if (!user.IsEmailVerified)
            {
                // Check if verification token expired and resend if needed
                if (user.EmailVerificationTokenExpiry < DateTime.UtcNow)
                {
                    // Generate new token
                    user.EmailVerificationToken = GenerateRandomToken();
                    user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);

                    await _userRepo.SaveChangesAsync();

                    // Send new verification email
                    var verificationLink = EmailLinkBuilder.BuildVerificationLink(
                        _config["AppUrl"],
                        user.Email,
                        user.EmailVerificationToken);
                    _ = Task.Run(() => _emailService.SendVerificationEmailAsync(user.Email, user.FullName, verificationLink));
                }

                return new AuthResponse
                {
                    Success = false,
                    Message = "Please verify your email address before logging in. A verification link has been sent to your email.",
                    RequiresEmailVerification = true,
                    Email = user.Email
                };
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            _userRepo.Update(user);
            await _userRepo.SaveChangesAsync();

            // Clear failed attempts on successful login
            _cache.Remove(cacheKey);

            return await GenerateAuthResponse(user);
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            string cacheKey = $"user_{id}";

            // Try to get from cache
            if (_cache.TryGetValue(cacheKey, out User? cachedUser))
                return cachedUser;

            // Get from database
            var user = await _userRepo.GetByIdAsync(id);

            // Cache if found
            if (user != null)
            {
                _cache.Set(cacheKey, user, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                    SlidingExpiration = TimeSpan.FromMinutes(2)
                });
            }

            return user;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _userRepo.GetByEmailAsync(email);
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = await _userRepo.GetByEmailAsync(email);

            // Don't reveal if email exists for security
            if (user == null)
                return true;

            // Only send reset email if email is verified
            if (!user.IsEmailVerified)
                return true;

            // Generate password reset token
            var resetToken = GenerateRandomToken();

            // Store token in user record (not just cache for persistence)
            user.PasswordResetToken = resetToken;
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);

            _userRepo.Update(user);
            await _userRepo.SaveChangesAsync();

            // Send password reset email
            var resetLink = EmailLinkBuilder.BuildPasswordResetLink(
                _config["AppUrl"],
                user.Email,
                resetToken);

            await _emailService.SendPasswordResetEmailAsync(user.Email, user.FullName, resetLink);

            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
        {
            // Get user
            var user = await _userRepo.GetByEmailAsync(request.Email);

            if (user == null)
                return false;

            // Check if email is verified
            if (!user.IsEmailVerified)
                return false;

            // Verify reset token
            if (user.PasswordResetToken != request.Token)
                return false;

            // Check if token expired
            if (user.PasswordResetTokenExpiry < DateTime.UtcNow)
                return false;

            // Update password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;

            _userRepo.Update(user);
            await _userRepo.SaveChangesAsync();

            return true;
        }

        public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                // Find user by refresh token
                var user = await _userRepo.GetUserByRefreshTokenAsync(refreshToken);
                if (user == null)
                    throw new Exception("Invalid refresh token");

                var tokenEntity = user.RefreshTokens.FirstOrDefault(t => t.Token == refreshToken);
                if (tokenEntity == null || tokenEntity.IsRevoked || tokenEntity.IsUsed || tokenEntity.ExpiryDate < DateTime.UtcNow)
                {
                    throw new Exception("Refresh token is expired, revoked, or already used");
                }

                // Check if user is active
                if (!user.IsActive)
                    throw new Exception("Account is deactivated");

                // Check if email is verified
                if (!user.IsEmailVerified)
                    throw new Exception("Email not verified");

                // Mark current token as used/revoked
                tokenEntity.IsUsed = true;
                tokenEntity.RevokedAt = DateTime.UtcNow;

                // Generate new tokens
                return await GenerateAuthResponse(user);
            }
            catch (Exception ex)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = $"Token refresh failed: {ex.Message}"
                };
            }
        }

        public async Task<bool> LogoutAsync(Guid userId)
        {
            // Clear user from cache
            _cache.Remove($"user_{userId}");

            // Revoke all refresh tokens in database
            var user = await _userRepo.GetUserWithRefreshTokensAsync(userId);
            if (user != null)
            {
                foreach (var token in user.RefreshTokens.Where(t => !t.IsRevoked && !t.IsUsed))
                {
                    token.IsRevoked = true;
                    token.RevokedAt = DateTime.UtcNow;
                }
                _userRepo.Update(user);
                await _userRepo.SaveChangesAsync();
            }

            return true;
        }

        // NEW: Verify email with token
        public async Task<bool> VerifyEmailAsync(string email, string token)
        {
            var user = await _userRepo.GetByEmailAsync(email);

            if (user == null)
                return false;

            if (user.IsEmailVerified)
                return false;

            if (user.EmailVerificationToken != token)
                return false;

            if (user.EmailVerificationTokenExpiry < DateTime.UtcNow)
                return false;

            // Mark email as verified
            user.IsEmailVerified = true;
            user.EmailVerifiedAt = DateTime.UtcNow;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;

            _userRepo.Update(user);
            await _userRepo.SaveChangesAsync();

            return true;
        }

        // NEW: Resend verification email
        public async Task<bool> ResendVerificationEmailAsync(string email)
        {
            var user = await _userRepo.GetByEmailAsync(email);

            if (user == null)
                return false;

            if (user.IsEmailVerified)
                return false;

            // Generate new verification token
            user.EmailVerificationToken = GenerateRandomToken();
            user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);

            _userRepo.Update(user);
            await _userRepo.SaveChangesAsync();

            // Send verification email
            var verificationLink = EmailLinkBuilder.BuildVerificationLink(
                _config["AppUrl"],
                user.Email,
                user.EmailVerificationToken);

            await _emailService.SendVerificationEmailAsync(user.Email, user.FullName, verificationLink);

            return true;
        }

        // Generate JWT Token and Response
        private async Task<AuthResponse> GenerateAuthResponse(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["JWT:Secret"] ?? "your-super-secret-key-minimum-32-characters-long");

            // Check if user has a vendor profile and fetch it if details exist
            var userWithDetails = await _userRepo.GetUserWithDetailsAsync(user.Id);
            var vendor = userWithDetails?.Vendor;

            // Create claims
            var claims = new List<Claim>
            {
                new Claim("userId", user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("isActive", user.IsActive.ToString()),
                new Claim("isEmailVerified", user.IsEmailVerified.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat,
                    new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64)
            };

            if (vendor != null)
            {
                claims.Add(new Claim("vendorId", vendor.Id.ToString()));
                claims.Add(new Claim("vendorStatus", vendor.Status.ToString()));
                claims.Add(new Claim("isVendorApproved", vendor.IsApproved.ToString()));
            }

            // Create token descriptor
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["JWT:ExpiryInMinutes"] ?? "60")),
                Issuer = _config["JWT:Issuer"],
                Audience = _config["JWT:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            // Generate token
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // Generate refresh token and save to database
            var refreshTokenString = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshTokenString,
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false,
                IsUsed = false
            };
            user.RefreshTokens.Add(refreshTokenEntity);
            await _userRepo.SaveChangesAsync();

            return new AuthResponse
            {
                Success = true,
                Token = tokenString,
                RefreshToken = refreshTokenString,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role.ToString(),
                UserId = user.Id,
                TokenExpiry = tokenDescriptor.Expires ?? DateTime.UtcNow.AddMinutes(60),
                IsEmailVerified = user.IsEmailVerified,
                Message = "Authentication successful"
            };
        }

        // Extract claims from expired token
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var key = Encoding.UTF8.GetBytes(_config["JWT:Secret"] ?? "your-super-secret-key-minimum-32-characters-long");

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = false, // Don't validate expiration for refresh
                ValidIssuer = _config["JWT:Issuer"],
                ValidAudience = _config["JWT:Audience"],
                ClockSkew = TimeSpan.Zero
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }

        // Helper method to generate random token
        private string GenerateRandomToken()
        {
            return Guid.NewGuid().ToString("N");
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) return false;

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            
            // Revoke all refresh tokens (logout from other devices on password change!)
            var userWithTokens = await _userRepo.GetUserWithRefreshTokensAsync(userId);
            if (userWithTokens != null)
            {
                foreach (var token in userWithTokens.RefreshTokens.Where(t => !t.IsRevoked && !t.IsUsed))
                {
                    token.IsRevoked = true;
                    token.RevokedAt = DateTime.UtcNow;
                }
                _userRepo.Update(userWithTokens);
            }
            else
            {
                _userRepo.Update(user);
            }

            await _userRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateProfileAsync(Guid userId, UpdateUserProfileRequest request)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) return false;

            user.FullName = request.FullName;
            user.Phone = request.Phone ?? string.Empty;
            user.Bio = request.Bio;
            user.ProfilePictureUrl = request.ProfilePictureUrl;
            user.UpdatedAt = DateTime.UtcNow;

            _userRepo.Update(user);
            await _userRepo.SaveChangesAsync();

            // Clear cache
            _cache.Remove($"user_{userId}");
            return true;
        }

        public async Task<object?> GetUserProfileWithDetailsAsync(Guid userId)
        {
            var user = await _userRepo.GetUserWithDetailsAsync(userId);
            if (user == null) return null;

            return new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.Phone,
                user.Role,
                user.IsActive,
                user.IsEmailVerified,
                user.ProfilePictureUrl,
                user.Bio,
                user.CreatedAt,
                user.LastLoginAt,
                Addresses = user.Addresses.Select(a => new
                {
                    a.Id,
                    a.AddressLine1,
                    a.AddressLine2,
                    a.City,
                    a.State,
                    a.Country,
                    a.PostalCode,
                    a.Landmark,
                    a.PhoneNumber,
                    a.RecipientName,
                    a.IsDefault,
                    a.AddressType
                }).ToList(),
                Vendor = user.Vendor != null ? new
                {
                    user.Vendor.Id,
                    user.Vendor.ShopName,
                    user.Vendor.ShopSlug,
                    user.Vendor.Description,
                    user.Vendor.LogoUrl,
                    user.Vendor.Status,
                    user.Vendor.IsApproved,
                    user.Vendor.ApprovedAt
                } : null
            };
        }
    }
}