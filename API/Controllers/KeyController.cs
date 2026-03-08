using API.DTOs;
using API.DTOs.VehicleTypeDto;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using UnitOfWork;

namespace API.Controllers
{[AllowAnonymous]
 
    public class KeyController : BaseGenericApiController<Key, KeyDto, KeyDto>
    {

         public KeyController(IUnitOfWork uow) : base(uow)
        {
        }
    }
}