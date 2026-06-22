using EcommerceApp.Domain.Entities.Common;
using System.Linq.Expressions;

namespace EcommerceApp.Application.Interfaces.Repositories;

public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<IReadOnlyList<T>> GetWhereAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    T Update(T entity);
    void Delete(T entity);
    Task<int> SaveChangesAsync();
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
}
