using EcommerceApp.Application.Interfaces.Repositories;
using EcommerceApp.Domain.Entities.Common;
using EcommerceApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EcommerceApp.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext DbContext;
    protected readonly DbSet<T> DbSet;

    public GenericRepository(ApplicationDbContext dbContext)
    {
        DbContext = dbContext;
        DbSet = dbContext.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id) => await DbSet.FindAsync(id);

    public virtual async Task<IReadOnlyList<T>> GetAllAsync() => await DbSet.AsNoTracking().ToListAsync();

    public virtual async Task<IReadOnlyList<T>> GetWhereAsync(Expression<Func<T, bool>> predicate) =>
        await DbSet.AsNoTracking().Where(predicate).ToListAsync();

    public virtual async Task<T> AddAsync(T entity)
    {
        await DbSet.AddAsync(entity);
        return entity;
    }

    public virtual T Update(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        DbSet.Update(entity);
        return entity;
    }

    public virtual void Delete(T entity) => DbSet.Remove(entity);

    public Task<int> SaveChangesAsync() => DbContext.SaveChangesAsync();

    public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) => DbSet.AnyAsync(predicate);
}
