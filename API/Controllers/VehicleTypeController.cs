using API.DTOs.VehicleTypeDto;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using UnitOfWork;

namespace API.Controllers
{
    
    public class VehicleTypeController : BaseGenericApiController<VehicleType, VehicleTypeAddDto, VehicleTypeReturnDto>
    {
        public VehicleTypeController(IUnitOfWork uow) : base(uow)
        {
        }
    }
}