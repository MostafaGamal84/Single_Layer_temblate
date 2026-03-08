using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Interfaces
{
    public interface IRepository<TEntity> where TEntity : BaseEntity
    {
        TEntity Add(TEntity entity);
        void Remove(TEntity entity);
        void AddRange(List<TEntity> entity);
        void Update(TEntity entity);

        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<IEnumerable<TEntity>> GetAllByAsync(Expression<Func<TEntity, bool>> expression);

        Task<TEntity> GetByIdAsync(int id);
        TEntity GetBy(Expression<Func<TEntity, bool>> expression);
        Task<TEntity> GetByAsync(Expression<Func<TEntity, bool>> expression);

        // Task<TEntity> GetEntityWithSpec (ISpecification<TEntity> spec);
        // Task<IReadOnlyList<TEntity>> ListAsync(ISpecification<TEntity> spec);


        Task<IEnumerable<T>> Map_GetAllAsync<T>();
        Task<IEnumerable<T>> Map_GetAllByAsync<T>(Expression<Func<TEntity, bool>> expression);
        // Task<IEnumerable<T>> Map_GetAllByAsyncX<T>(Expression<Func<TEntity, bool>> expression, Expression<Func<TEntity, object>> orderBy = null);
        Task<T> Map_GetByAsync<T>(Expression<Func<TEntity, bool>> expression);
    }
}