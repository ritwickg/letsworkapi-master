using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LetsWork.Domain.Interfaces.Repositories
{
    public interface IAsyncRepository<T> where T : class, new()
    {
        Task<T> GetByIdAsync(Guid id);
        Task<T> GetSingleBySpecAsync(Expression<Func<T, bool>> Criteria);
        Task<List<T>> ListAllAsync();
        Task<List<T>> ListAsync(Expression<Func<T, bool>> Criteria);
        Task AddAsync(T Entity);
        Task UpdateAsync(T Entity);
        Task DeleteAsync(T Entity);
    }
}
