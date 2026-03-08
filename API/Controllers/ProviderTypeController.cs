using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs.ProviderDto;
using API.Entities;
using API.Error;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UnitOfWork;

namespace API.Controllers
{
   [AllowAnonymous]
    public class ProviderTypeController : BaseGenericApiController<ProviderType, ProviderTypeAddDto, ProviderTypeReturnDto>
    {
           private readonly IUnitOfWork _uow;
        private readonly IRepository<ProviderType> _ProviderTypeRepo;
        public ProviderTypeController(IUnitOfWork uow) : base(uow)
        {
            _uow = uow;
            _ProviderTypeRepo = _uow.Repository<ProviderType>();
        }

        
        [HttpGet("GetPerson")]
        public async Task<IActionResult> GetPerson()
        {
            var result = await _ProviderTypeRepo.Map_GetAllByAsync<ProviderTypeReturnDto>(x => x.Name_en == "Person");

            return Ok(result);
        }
    }
}