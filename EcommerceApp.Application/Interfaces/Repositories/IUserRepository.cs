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

    Task<int> SaveChangesAsync();

    Task<User?> GetUserWithRefreshTokensAsync(Guid userId);
    Task<User?> GetUserWithAddressesAsync(Guid userId);
}