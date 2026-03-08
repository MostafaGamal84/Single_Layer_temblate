using API.Data;
using API.Entities;
using API.IRepository;
using AutoMapper;

namespace API.Repository
{
    public class AppUserTypeRepo : BaseRepo<AppUserType>, IAppUserTypeRepo
    {
        public AppUserTypeRepo(DataContext context, IMapper mapper) : base(context, mapper)
        {
        }
    }
}