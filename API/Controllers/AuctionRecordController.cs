using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using API.DTOs;
using API.DTOs.AppPercentDto;
using API.DTOs.AuctionDto;
using API.DTOs.AuctionRecordDto;
using API.DTOs.EndedAuctionsDto;
using API.Entities;
using API.Entities.Auctions;
using API.Error;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UnitOfWork;

namespace API.Controllers
{

    public class AuctionRecordController : BaseGenericApiController<AuctionRecord, AuctionRecordAddDto, AuctionRecordReturnDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IRepository<AuctionRecord> _rec;
        private readonly IRepository<Auction> _auc;
        private readonly IRepository<Key> _Key;
        private readonly IRepository<CarCommetion> _CarCommetion;

        public AuctionRecordController(IUnitOfWork uow) : base(uow)
        {
            _uow = uow;
            _rec = _uow.Repository<AuctionRecord>();
            _auc = _uow.Repository<Auction>();
            _Key = _uow.Repository<Key>();
            _CarCommetion = _uow.Repository<CarCommetion>();

        }


        [HttpPost("add")]
        public override async Task<IActionResult> Add(AuctionRecordAddDto dto)
        {
            var recorded = await _rec.Map_GetAllByAsync<AuctionRecordReturnDto>(x => x.AuctionId == dto.AuctionId && x.ClientId == User.GetUserId());
            if (recorded.Count() == 0)
            {
                var user = await _uow.UserRepo.GetById(User.GetUserId());
                if (user.SubscribeCount != 0)
                {
                    user.SubscribeCount = user.SubscribeCount - 1;
                    _uow.UserRepo.Update(user);
                    if (!await _uow.SaveAsync()) return BadRequest("Error Update");
                }
                else
                {
                    return BadRequest("Subscribe Count value is zero");
                }

            }
            dto.Id = 0;
            var percent = await _Key.Map_GetByAsync<KeyDto>(x => x.Name == "Percent");


            var x = _uow.Mapper.Map<AuctionRecord>(dto);


            x.ClientId = User.GetUserId();

            x.CreatedAt = DateTime.Now;
            x.AppPercent = x.Price * percent.Value / 100;
            x.Percent = percent.Value;


            var result = _rec.Add(x);



            if (!await _uow.SaveAsync()) return BadRequest(new ApiResponse(400));

            var map = _uow.Mapper.Map<AuctionRecordReturnDto>(result);

            return Ok(map);
        }

        [HttpPost("CheckCarCommesionPayed/{auctionId}")]
        public async Task<IActionResult> Add(int auctionId)
        {
            var user = User.GetUserId();
            var recorded = await _CarCommetion.Map_GetAllByAsync<CarCommesionDto>(x => x.AuctionId == auctionId && x.ClientId == user);
            if (recorded.Count() == 0)
            {
                return BadRequest("You didnt pay car commetion yet");
            }
            return Ok(" You payed this commesion");
        }

        [HttpPost("addCarCommesion/{auctionId}/{carCommesion}")]
        public async Task<IActionResult> AddCarCommesion(int auctionId, double carCommesion)
        {
            var user = User.GetUserId();

            CarCommetion car = new CarCommetion();
            car.AuctionId = auctionId;
            car.ClientId = user;
            car.CarReportCommission = carCommesion;
            var result = _CarCommetion.Add(car);
            if (!await _uow.SaveAsync()) return BadRequest(400);
            var map = _uow.Mapper.Map<CarCommesionDto>(result);
            return Ok(map);

        }


        [HttpGet("GetCarCommesion/{auctionId}")]
        public async Task<IActionResult> GetCarCommesionById(int auctionId)
        {
            var result = await _CarCommetion.Map_GetAllByAsync<CarCommesionDto>(x => x.AuctionId == auctionId);

            return Ok(result);
        }
        [HttpGet("GetCarCommesion")]
        public async Task<IActionResult> GetCarCommesion()
        {
            var result = await _CarCommetion.Map_GetAllAsync<CarCommesionDto>();
            var Total = result.Sum(z => z.CarReportCommission);


            return Ok(new { result, Total });


        }

        [AllowAnonymous]
        [HttpPut("PlusAndMinus")]
        public async Task<IActionResult> PlusAndMinus(PlusAndMinusDto dto)
        {
            var entity = await _rec.GetByAsync(x => x.Id == dto.Id);

            if (entity == null) return NotFound(new ApiResponse(StatusCodes.Status404NotFound));

            var result = _uow.Mapper.Map(dto, entity);

            result.IsDeleted = true;

            _rec.Update(result);

            if (await _uow.SaveAsync()) return Ok();

            return BadRequest("Failed to Update");
        }

        [HttpGet("GetByAuctionId/{id}")]
        public async Task<IActionResult> GetByAuctionId(int id)
        {
            var result = await _rec.Map_GetAllByAsync<AuctionRecordReturnDto>(x => x.AuctionId == id);

            return Ok(result);
        }

        [HttpGet("GetByUserId/{id}/{auctionId}")]
        public async Task<IActionResult> GetByUserId(int id, int auctionId)
        {
            var result = await _rec.Map_GetAllByAsync<AuctionRecordReturnDto>(x => x.ClientId == id);

            return Ok(result);
        }
        [HttpGet("getListOfAuctionsByUserId/{id}")]
        public async Task<IActionResult> GetListOfAuctionsByUserId(int id)
        {
            var result = await _rec.Map_GetAllByAsync<AuctionListDto>(x => x.ClientId == id);

            return Ok(result);
        }



        [AllowAnonymous]
        [HttpGet("AuctionReport")]
        public async Task<IActionResult> GetAuctionReport()
        {
            var result = await _auc.Map_GetAllByAsync<EndedAuctionReturnDto>
            (x => x.AuctionRecords.Count > 0);


            var Total = result.Sum(z => z.Insurance);



            return Ok(new { result, Total });

        }
    }
}