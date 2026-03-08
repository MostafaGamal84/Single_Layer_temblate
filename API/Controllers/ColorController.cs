using API.DTOs.ColorDto;
using API.Entities.Auctions;
using Microsoft.AspNetCore.Authorization;
using UnitOfWork;

namespace API.Controllers
{

   
    public class ColorController : BaseGenericApiController<Color, ColorAddDto, ColorReturnDto>
    {
        public ColorController(IUnitOfWork uow) : base(uow)
        {
        }
    }
}