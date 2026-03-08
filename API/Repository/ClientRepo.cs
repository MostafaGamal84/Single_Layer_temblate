using API.IRepository;
using AutoMapper;

namespace API.Repository
{
    public class ClientRepo : BaseRepo<Client>, IClientRepo
    {
        public ClientRepo(DataContext context, IMapper mapper) : base(context, mapper)
        {
            
        }
    }
}