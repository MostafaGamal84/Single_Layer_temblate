using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using UnitOfWork;

namespace API.Controllers
{
    [AllowAnonymous]
    public class AppUserTypeController : BaseGenericApiController<AppUserType, AppUserTypeDto, AppUserTypeDto>
    {
        public AppUserTypeController(IUnitOfWork uow) : base(uow)
        {
        }
    }
}