
using API.DTOs.GenderDTO;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using UnitOfWork;

namespace API.Controllers
{
    [AllowAnonymous]
    public class GenderController : BaseGenericApiController<Gender, GenderDto, GenderDto>
    {
        public GenderController(IUnitOfWork uow) : base(uow)
        {
        }
    }
}