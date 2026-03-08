
using System.Threading.Tasks;

using API.Error;
using API.Interfaces;
using DTOs;
using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UnitOfWork;

namespace API.Controllers
{
    // [AllowAnonymous]
    public class BaseGenericApiController<TEntity, TAddDto, TReturnDto> : BaseApiController
    where TEntity : BaseEntity
    where TAddDto : BaseDto
    where TReturnDto : BaseDto

    {
        private readonly IUnitOfWork _uow;
        private readonly IRepository<TEntity> _Repo;

        public BaseGenericApiController(IUnitOfWork uow)
        {
            _uow = uow;
            _Repo = _uow.Repository<TEntity>();
        }


        [HttpPost("add")]
        public virtual async Task<IActionResult> Add(TAddDto dto)
        {
            dto.Id = 0;

            var x = _uow.Mapper.Map<TEntity>(dto);

            var result = _Repo.Add(x);

            if (!await _uow.SaveAsync()) return BadRequest(new ApiResponse(400));

            var map = _uow.Mapper.Map<TReturnDto>(result);

            return Ok(map);
        }


        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost("adddd")]
        public virtual async Task<IActionResult> Add(TEntity entity)
        {
            var result = _Repo.Add(entity);

            if (!await _uow.SaveAsync()) return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest));

            var map = _uow.Mapper.Map<TReturnDto>(result);

            return Ok(map);
        }

        [HttpPut("update")]
        public virtual async Task<IActionResult> Update(TAddDto dto)
        {
            var entity = await _Repo.GetByAsync(x => x.Id == dto.Id);

            if (entity == null) return NotFound(new ApiResponse(StatusCodes.Status404NotFound));

            var result = _uow.Mapper.Map(dto, entity);

            _Repo.Update(result);
            

            if (await _uow.SaveAsync()) return Ok();

            return BadRequest("Failed to Update");
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPut("updateee")]

        public virtual async Task<IActionResult> Update(TEntity entity)
        {
            _Repo.Update(entity);

            if (!await _uow.SaveAsync()) return BadRequest("Failed to Update");

            return Ok();

        }

        [HttpDelete ("{id}")]
        public virtual async Task<ActionResult> Delete(int id)
        {
            var entity = await _Repo.GetByAsync(x => x.Id == id);

            if (entity == null) return NotFound(new ApiResponse(StatusCodes.Status404NotFound));

            entity.IsDeleted = true;

            _Repo.Update(entity);

            if (!await _uow.SaveAsync()) return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest));

            return Ok();


        }

        [HttpGet]
        public virtual async Task<IActionResult> Get()
        {
            var result = await _Repo.Map_GetAllByAsync<TReturnDto>(x => x.IsDeleted == false);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public virtual async Task<IActionResult> GetById(int id)
        {
            // var cityId = _hashids.Decode(id)[0];

            var result = await _Repo.Map_GetByAsync<TReturnDto>(x => x.Id == id);

            return Ok(result);
        }
    }
}