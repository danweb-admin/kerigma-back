using System;
using System.Linq.Expressions;
using RccManager.Domain.Entities;

namespace RccManager.Domain.Interfaces.Repositories
{
	public interface IRepository<T> where T : BaseEntity
	{
		Task<T> Insert(T entity);
        Task<T> Update(T entity);
        Task<IEnumerable<T>> GetAll(string search);
        Task<IQueryable<T>> Query(Expression<Func<T, bool>> filter);
        Task<bool> Delete(Guid id);
        Task<T> GetById(Guid id);

    }
}

