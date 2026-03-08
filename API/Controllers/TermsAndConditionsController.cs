using API.DTOs.TermsDto;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using UnitOfWork;

namespace API.Controllers
{
    [AllowAnonymous]

    public class TermsAndConditionsController : BaseGenericApiController<TermsAndConditions, TermsAndConditionsAddDto, TermsAndConditionsReturnDto>
    {
        public TermsAndConditionsController(IUnitOfWork uow) : base(uow)
        {
        }
    }
}