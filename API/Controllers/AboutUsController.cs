using API.DTOs.TermsDto;
using Microsoft.AspNetCore.Authorization;
using UnitOfWork;

namespace API.Controllers
{ [AllowAnonymous]
    public class AboutUsController : BaseGenericApiController<AboutUs, AboutUsAddDto, AboutUsReturnDto>
    {
        public AboutUsController(IUnitOfWork uow) : base(uow)
        {
        }
    }
}