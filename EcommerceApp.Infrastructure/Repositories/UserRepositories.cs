using EcommerceApp.Application.Interfaces.Repositories;
using EcommerceApp.Domain.Entities.Identity;
using EcommerceApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceApp.Infrastructure.Repositories
{
    internal class UserRepositories : IUserRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public UserRepositories(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task AddAsync(User user)
        {
            await _dbContext.Users.AddAsync(user);
        }
        //public async Task Remove(User user)
        //{

        //}
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbContext.Users
                .FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Users
                .FirstOrDefaultAsync(x => x.Id == id);
        }


        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _dbContext.Users
                .AnyAsync(x => x.Email == email);
        }

        public void Update(User user)
        {
            _dbContext.Users.Update(user);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<User?> GetUserWithRefreshTokensAsync(Guid userId)
        {
            return await _dbContext.Users
                .Include(x => x.RefreshTokens)
                .FirstOrDefaultAsync(x => x.Id == userId);
        }
        public async Task<User?> GetUserWithAddressesAsync(Guid userId)
        {
            return await _dbContext.Users
                .Include(x => x.Addresses)
                .FirstOrDefaultAsync(x => x.Id == userId);
        }

        public void Remove(User user)
        {
            _dbContext.Users.Remove(user);

        }

        //public async Task Remove(User user)
        //{
        //    return await _dbContext.Users.Remove(user);
        //}
    }
}
