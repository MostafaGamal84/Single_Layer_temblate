using API.DTOs;
using Microsoft.AspNetCore.Authorization;
using UnitOfWork;

namespace API.Controllers
{
    [AllowAnonymous]
    public class SubscribeController : BaseGenericApiController<Subscribe, SubscribeDto, SubscribeDto>
    {
       
        public SubscribeController(IUnitOfWork uow) : base(uow)
        {
        }
       
    }
}