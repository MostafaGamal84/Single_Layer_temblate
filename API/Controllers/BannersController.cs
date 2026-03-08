using API.DTOs.BannersDto;
using API.Error;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UnitOfWork;

namespace API.Controllers
{
    [AllowAnonymous]
    public class BannersController : BaseGenericApiController<Banners, BannersAddDto, BannersReturnDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IRepository<Banners> _bannersRepo;
        public BannersController(IUnitOfWork uow) : base(uow)
        {
            _uow = uow;
            _bannersRepo = _uow.Repository<Banners>();
        }

        [HttpPost("add")]
        public override async Task<IActionResult> Add(BannersAddDto dto)
        {
            dto.Id = 0;

            var x = _uow.Mapper.Map<Banners>(dto);

            var Start = x.StartAt.Date;

            x.StartAt = Start;

            var End = x.EndAt.Date;

            x.EndAt = End;

            x.PhotoUrl = await _uow.FileRepository.CreateFileFromBase64Async(dto.FileBase64, dto.Name_en);

            var result = _bannersRepo.Add(x);

            if (!await _uow.SaveAsync()) return BadRequest(new ApiResponse(400));

            var map = _uow.Mapper.Map<BannersReturnDto>(result);

            return Ok(map);
        }


         [HttpPut("update")]
        public override async Task<IActionResult> Update(BannersAddDto dto)
        {
            var entity = await _bannersRepo.GetByAsync(x => x.Id == dto.Id);

            if (entity == null) return NotFound(new ApiResponse(StatusCodes.Status404NotFound));

            var result = _uow.Mapper.Map(dto, entity);
            
            result.IsDeleted = true;

            _bannersRepo.Update(result);

            if (await _uow.SaveAsync()) return Ok();

            return BadRequest("Failed to Update");
        }

    }
}