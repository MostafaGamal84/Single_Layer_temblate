
using API.DTOs.BrandNameDto;
using API.Entities.Auctions;
using API.Error;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnitOfWork;

namespace API.Controllers
{
     [AllowAnonymous]
    public class BrandNameController : BaseGenericApiController<BrandName, BrandNameAddDto, BrandNameReturnDto>
    {
        private readonly IRepository<BrandName> _Brand;
        private readonly IUnitOfWork _uow;
        public BrandNameController(IUnitOfWork uow) : base(uow)
        {
            _uow = uow;
            _Brand = _uow.Repository<BrandName>();
        }
        
        [HttpPost("add")]
        public override async Task<IActionResult> Add(BrandNameAddDto dto)
        {
            dto.Id = 0;

            var x = _uow.Mapper.Map<BrandName>(dto);

            x.PhotoUrl = await _uow.FileRepository.CreateFileFromBase64Async(dto.FileBase64, dto.Name_en);
            
            var result = _Brand.Add(x);

            if (!await _uow.SaveAsync()) return BadRequest(new ApiResponse(400));

            var map = _uow.Mapper.Map<BrandNameReturnDto>(result);

            return Ok(map);
        }
    }
}