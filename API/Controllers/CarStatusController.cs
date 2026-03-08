using API.DTOs.CarStatusDto;
using API.Entities.Auctions;
using Microsoft.AspNetCore.Authorization;
using UnitOfWork;

namespace API.Controllers
{
    [AllowAnonymous]
    public class CarStatusController : BaseGenericApiController<CarStatus, CarStatusAddDto, CarStatusReturnDto>
    {
        public CarStatusController(IUnitOfWork uow) : base(uow)
        {
        }
    }
}