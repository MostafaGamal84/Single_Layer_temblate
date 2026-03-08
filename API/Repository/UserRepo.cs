using System.Collections.Generic;
using System.Threading.Tasks;
using Api.IRepositories;
using API.Entities;
using API.Interfaces;
using API.Repository;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepo : BaseRepo<Client>, IUserRepo
    {
        public UserRepo(DataContext context, IMapper mapper) : base(context, mapper)
        {
        }
    }
}