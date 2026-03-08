using API.Data;
using API.Entities;
using API.IRepository;
using AutoMapper;

namespace API.Repository
{
    public class AdminRepo : BaseRepo<Admin>, IAdminRepo
    {
        public AdminRepo(DataContext context, IMapper mapper) : base(context, mapper)
        {
        }
    }
}