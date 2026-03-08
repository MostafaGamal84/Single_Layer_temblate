using API.DTOs.CarTypeDto;
using API.Entities.Auctions;
using API.Error;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnitOfWork;

namespace API.Controllers
{
    
    public class CarTypeController : BaseGenericApiController<CarType, CarTypeAddDto, CarTypeReturnDto>
    {
       
        public CarTypeController(IUnitOfWork uow) : base(uow)
        {
        }
       
    }
}