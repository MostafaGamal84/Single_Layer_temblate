using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using API.Data;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Entities;

using Microsoft.EntityFrameworkCore;

namespace API.Repository
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public Repository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public TEntity Add(TEntity entity)
        {
            _context.Set<TEntity>().Add(entity);
            return entity;
        }


        public void AddRange(List<TEntity> entities)
        {
            _context.Set<TEntity>().AddRange(entities);
        }

        public void Update(TEntity entity)
        {
            _context.Update<TEntity>(entity);

        }

        public void Remove(TEntity entity)
        {
            _context.Set<TEntity>().Remove(entity);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _context.Set<TEntity>().ToListAsync();
        }

        public async Task<IEnumerable<TEntity>> GetAllByAsync(Expression<Func<TEntity, bool>> expression)
        {
            return await _context.Set<TEntity>().Where(expression).ToListAsync();
        }

        public TEntity GetBy(Expression<Func<TEntity, bool>> expression)
        {
            return _context.Set<TEntity>().FirstOrDefault(expression);
        }
        public async Task<TEntity> GetByAsync(Expression<Func<TEntity, bool>> expression)
        {
            return await _context.Set<TEntity>().FirstOrDefaultAsync(expression);
        }



        public async Task<TEntity> GetByIdAsync(int id)
        {
            return await _context.Set<TEntity>().FindAsync(id);
        }

        // public async Task<TEntity> GetEntityWithSpec(ISpecification<TEntity> spec)
        // {
        //     var x = await ApplySpecification(spec).FirstOrDefaultAsync();

        //     return x;
        // }

        // public async Task<IReadOnlyList<TEntity>> ListAsync(ISpecification<TEntity> spec)
        // {
        //     var x = await ApplySpecification(spec).ToListAsync();

        //     return x;
        // }

        // private IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> spec)
        // {
        //     // return SpecificationsEvaluator<TEntity>.GetQuery(_context.Set<TEntity>().AsQueryable(), spec);
        // }

        public async Task<IEnumerable<T>> Map_GetAllAsync<T>()
        {
            return await _context.Set<TEntity>()
                .ProjectTo<T>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<IEnumerable<T>> Map_GetAllByAsync<T>(Expression<Func<TEntity, bool>> expression)
        {
            return await _context.Set<TEntity>()
                .Where(expression)
                .ProjectTo<T>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        // public async Task<IEnumerable<T>> Map_GetAllByAsyncX<T>(Expression<Func<TEntity, bool>> expression, Expression<Func<TEntity, object>> orderBy = null)
        // {
        //     return await _context.Set<TEntity>()
        //         .Where(expression)
        //         .OrderByDescending(orderBy)
        //         .ProjectTo<T>(_mapper.ConfigurationProvider)
        //         .ToListAsync();
        // }

        public async Task<T> Map_GetByAsync<T>(Expression<Func<TEntity, bool>> expression)
        {
            return await _context.Set<TEntity>()
                .Where(expression)
                .ProjectTo<T>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();


        }
    }
}