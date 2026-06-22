using EcommerceApp.Domain.Entities.Identity;

namespace EcommerceApp.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> ExistsByEmailAsync(string email);

    Task AddAsync(User user);
    void Update(User user);
    void Remove(User user);
    Task AddRefreshTokenAsync(RefreshToken refreshToken);

    Task<int> SaveChangesAsync();

    Task<User?> GetUserWithRefreshTokensAsync(Guid userId);
    Task<User?> GetUserWithAddressesAsync(Guid userId);
    Task<User?> GetUserByRefreshTokenAsync(string refreshToken);
    Task<User?> GetUserWithDetailsAsync(Guid userId);
}