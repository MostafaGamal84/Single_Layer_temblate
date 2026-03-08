using API.DTOs.ModelDto;
using API.Entities.Auctions;
using Microsoft.AspNetCore.Authorization;
using UnitOfWork;

namespace API.Controllers
{
    [AllowAnonymous]
    public class ModelController : BaseGenericApiController<Model, ModelAddDto, ModelReturnDto>
    {
        public ModelController(IUnitOfWork uow) : base(uow)
        {
        }
    }
}