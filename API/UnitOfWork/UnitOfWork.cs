using System;
using System.Collections;
using System.Threading.Tasks;
using Api.IRepositories;
using API.Data;
using API.Entities;
using API.Interfaces;
using API.IRepository;
using API.Repository;
using AutoMapper;

using Entities;
using Microsoft.AspNetCore.Identity;

namespace UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {

        private readonly IHostEnvironment _hostEnvironment;

        private Hashtable _repositories;
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public UnitOfWork(DataContext context, IMapper mapper, IHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
            _mapper = mapper;
            _context = context;
        }

        public IMapper Mapper => _mapper;

        public IProvidersRepo ProvidersRepo => new ProvidersRepo(_context, _mapper);
        public IUserRepo UserRepo => new UserRepo(_context, _mapper);
        public IAdminRepo AdminRepo => new AdminRepo(_context, _mapper);
        public IClientRepo ClientRepo => new ClientRepo(_context, _mapper);
        public IFileRepository FileRepository => new FileRepository(_hostEnvironment);
        public UserManager<AppUser> UserManager => throw new NotImplementedException();
        public async Task<bool> SaveAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public bool HasChanges()
        {
            return _context.ChangeTracker.HasChanges();
        }

        public IRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
        {
            if (_repositories == null) _repositories = new Hashtable();

            var type = typeof(TEntity).Name;

            if (!_repositories.ContainsKey(type))
            {
                var repositoryType = typeof(Repository<>);
                var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(TEntity)), _context, _mapper);

                _repositories.Add(type, repositoryInstance);
            }

            return (API.Repository.Repository<TEntity>)_repositories[type];
        }


    }
}