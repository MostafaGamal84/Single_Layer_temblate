using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace API.Interfaces
{
    public interface IBaseRepo<TEntity> where TEntity : class
    {
        void AddAsync(TEntity entity);
        void Remove(TEntity entity);
        void AddRangeAsync(List<TEntity> entity);
        void Update(TEntity entity);
        void UpdateAsync(TEntity entity);

        Task<IEnumerable<TEntity>> GetAll();
        Task<IEnumerable<TEntity>> GetAllBy(Expression<Func<TEntity, bool>> expression);

        Task<TEntity> GetById(int id);
    
        Task<TEntity> GetBy(Expression<Func<TEntity, bool>> expression);

        Task<IEnumerable<T>> Map_GetAll<T>();
        Task<IEnumerable<T>> Map_GetAllBy<T>(Expression<Func<T, bool>> expression);

        Task<IEnumerable<T>> Map_GetAllByX<T>(Expression<Func<TEntity, bool>> expression);

         Task<T> Map_GetBy<T>(Expression<Func<T, bool>> expression);

    }
}