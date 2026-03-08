using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using API.Data;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Repository
{
    public class BaseRepo<TEntity> : IBaseRepo<TEntity> where TEntity : class
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public BaseRepo(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public void AddAsync(TEntity entity)
        {
            _context.Set<TEntity>().Add(entity);
        }

        public void AddRangeAsync(List<TEntity> entities)
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

        public async Task<IEnumerable<TEntity>> GetAll()
        {
            return await _context.Set<TEntity>().ToListAsync();
        }

        public async Task<IEnumerable<TEntity>> GetAllBy(Expression<Func<TEntity, bool>> expression)
        {
            return await _context.Set<TEntity>().Where(expression).ToListAsync();
        }

        public async Task<TEntity> GetBy(Expression<Func<TEntity, bool>> expression)
        {
            return await _context.Set<TEntity>().FirstOrDefaultAsync(expression);

        }

        public async Task<TEntity> GetById(int id)
        {
            return await _context.Set<TEntity>().FindAsync(id);
        }
        public async Task<TEntity> GetByUid(string uid)
        {
            return await _context.Set<TEntity>().FindAsync(uid);
        }

        public async Task<IEnumerable<T>> Map_GetAll<T>()
        {
            return await _context.Set<TEntity>()
                .ProjectTo<T>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<IEnumerable<T>> Map_GetAllBy<T>(Expression<Func<T, bool>> expression)
        {
            return await _context.Set<TEntity>()
                .ProjectTo<T>(_mapper.ConfigurationProvider)
                .Where(expression)
                .ToListAsync();
        }

        public async Task<IEnumerable<T>> Map_GetAllByX<T>(Expression<Func<TEntity, bool>> expression)
        {
            return await _context.Set<TEntity>()
                .Where(expression)
                .ProjectTo<T>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<T> Map_GetBy<T>(Expression<Func<T, bool>> expression)
        {
            return await _context.Set<TEntity>()
                .ProjectTo<T>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(expression);
        }

        public void UpdateAsync(TEntity entity)
        {
            throw new NotImplementedException();
        }
    }
}