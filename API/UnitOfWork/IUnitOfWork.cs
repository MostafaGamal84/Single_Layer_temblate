using System;
using System.Threading.Tasks;
using Api.IRepositories;
using API.Entities;
using API.Interfaces;
using API.IRepository;
using AutoMapper;
using Entities;
using Microsoft.AspNetCore.Identity;

namespace UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;
        IProvidersRepo ProvidersRepo { get; }
        IAdminRepo AdminRepo { get; }
        IClientRepo ClientRepo { get; }
        IUserRepo UserRepo { get; }
        Task<bool> SaveAsync();
        bool HasChanges();
        IMapper Mapper { get; }
        IFileRepository FileRepository { get; }
        UserManager<AppUser> UserManager { get; }
    }
}