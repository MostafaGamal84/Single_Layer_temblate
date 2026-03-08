using API.Data;
using API.Entities;
using API.IRepository;
using AutoMapper;

namespace API.Repository
{
    public class ProvidersRepo : BaseRepo<Provider>, IProvidersRepo
    {
        public ProvidersRepo(DataContext context, IMapper mapper) : base(context, mapper)
        {
        }
    }
}