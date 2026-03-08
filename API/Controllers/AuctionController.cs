
using System.Threading.Tasks;
using API.DTOs;
using API.DTOs.AppPercentDto;
using API.DTOs.AuctionDto;
using API.DTOs.EndedAuctionsDto;
using API.DTOs.ProviderDto;
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

    public class AuctionController : BaseGenericApiController<Auction, AuctionAddDto, AuctionReturnDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IRepository<Auction> _auctionRepo;
        private readonly IRepository<ProviderType> _ProviderTypeRepo;

        public AuctionController(IUnitOfWork uow) : base(uow)
        {
            _uow = uow;
            _auctionRepo = _uow.Repository<Auction>();

            _ProviderTypeRepo = _uow.Repository<ProviderType>();
        }

        [HttpGet("GetPerson")]
        public async Task<IActionResult> GetPerson()
        {
            var result = await _ProviderTypeRepo.Map_GetAllByAsync<ProviderTypeReturnDto>(x => x.Name_en == "Person");

            return Ok(result);
        }

        [HttpPost("addByAdmin")]
        public override async Task<IActionResult> Add(AuctionAddDto dto)
        {
            dto.Id = 0;

            var x = _uow.Mapper.Map<Auction>(dto);

            x.IsApproved = true;

            x.Expired = false;

            x.IsDeleted = false;
            // x.StartAt = dto.StartA();
            // x.EndAt = dto.EndAt.ToUniversalTime();

            x.ProviderId = dto.ProviderId;

            x.AdminId = User.GetUserId();
            x.CarReport = await _uow.FileRepository.CreateFileFromBase64Async(dto.CarReportBase64, User.GetUserName());
            var photos = new List<ItemPhoto>();

            foreach (var row in dto.itemPhotos)
            {
                var xx = new ItemPhoto();

                xx.PhotoUrl = await _uow.FileRepository

                .CreateFileFromBase64Async(row.FileBase64, User.GetUserName());

                photos.Add(xx);
            }

            x.itemPhotos = photos;

            return await base.Add(x);
        }

        [HttpPost("addByProvider")]
        public async Task<IActionResult> AddByProvider(AuctionAddDto dto)
        {
            dto.Id = 0;


            var x = _uow.Mapper.Map<Auction>(dto);

            x.ProviderId = User.GetUserId();

            // x.StartAt = dto.StartAt.ToUniversalTime();

            // x.EndAt = dto.EndAt.ToUniversalTime();

            x.IsApproved = false;

            x.Expired = false;

            var photos = new List<ItemPhoto>();

            foreach (var row in dto.itemPhotos)
            {
                var xx = new ItemPhoto();

                xx.PhotoUrl = await _uow.FileRepository

                .CreateFileFromBase64Async(row.FileBase64, User.GetUserName());

                photos.Add(xx);
            }

            x.itemPhotos = photos;

            return await base.Add(x);
        }
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DEleteAuction(int id)
        {
            var auction = _auctionRepo.GetBy(x => x.Id == id);

            auction.IsDeleted = true;

            _auctionRepo.Update(auction);

            if (!await _uow.SaveAsync()) return BadRequest("error");

            return Ok();
        }
        
        [AllowAnonymous]
        [HttpPut("UpdateEndDate")]
        public async Task<IActionResult> UpdateStatusTrue(AuctionAddDto dto)
        {
            var auction = _auctionRepo.GetBy(x => x.Id == dto.Id);

            auction.EndAt = dto.EndAt;

            _auctionRepo.Update(auction);

            if (!await _uow.SaveAsync()) return BadRequest("error");

            return Ok();
        }


        [HttpPut("approvedIsTrue/{id}")]
        public async Task<IActionResult> UpdateStatusTrue(int id)
        {
            var auction = _auctionRepo.GetBy(x => x.Id == id);

            auction.IsApproved = true;

            auction.IsDeleted = false;

            _auctionRepo.Update(auction);

            if (!await _uow.SaveAsync()) return BadRequest("error");

            return Ok();
        }

        [HttpPut("approvedIsFalse/{id}")]
        public async Task<IActionResult> UpdateStatusFalse(int id)
        {
            var auction = _auctionRepo.GetBy(x => x.Id == id);

            auction.Expired = true;

            _auctionRepo.Update(auction);

            if (!await _uow.SaveAsync()) return BadRequest("error");

            return Ok();
        }

        [HttpPut("expired/{id}")]
        public async Task<IActionResult> UpdateStatusExpired(int id)
        {
            var auction = _auctionRepo.GetBy(x => x.Id == id);

            auction.Expired = true;

            _auctionRepo.Update(auction);

            if (!await _uow.SaveAsync()) return BadRequest("error");

            return Ok();
        }

        [AllowAnonymous]
        [HttpGet("getAuctionDetailsByAuctionId/{id}")]
        public async Task<IActionResult> GetAuctionDetailsByAuctionId(int id)
        {

            var auction = await _auctionRepo.Map_GetByAsync<AuctionReturnDto>(x => x.Id == id);

            return Ok(auction);
        }


        [AllowAnonymous]

        [HttpGet("getAllRecordEndedByAdmin")]
        public async Task<IActionResult> GetAllRecordEndedByAdmin()
        {
            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time");
            DateTime DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzi); // convert from utc to local

            var result = await _auctionRepo.Map_GetAllByAsync<EndedAuctionReturnDto>
            (x => x.EndAt < DateTime || x.Expired == true || x.IsDeleted == true);

            return Ok(result);
        }

        [HttpGet("getAllRecordEndedByProvider")]
        public async Task<IActionResult> GetAllRecordEndedByProvider()
        {
            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time");
            DateTime DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzi); // convert from utc to local

            var result = await _auctionRepo.Map_GetAllByAsync<EndedAuctionReturnDto>
            (x => x.ProviderId == User.GetUserId() &&
            (x.EndAt < DateTime || x.Expired == true || x.IsDeleted == true));

            return Ok(result);
        }

        [HttpGet("getAllRecordPercent")]
        public async Task<IActionResult> GetAllRecordPercent()
        {

            var result = await _auctionRepo.Map_GetAllByAsync<EndedAuctionReturnDto>
            (x => x.AuctionRecords.Count > 0);


            object x = new
            {
                Total = result.Sum(z => z.Price),

                AppPercent = result.Sum(z => z.FinalTotal)
            };

            return Ok(new { result, sum = x });

        }


        [AllowAnonymous]

        [HttpGet("getAllEndedByAdmin")]
        public async Task<IEnumerable<AuctionReturnDto>> GetAllEndedByAdmin()
        {
            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time");
            DateTime DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzi); // convert from utc to local

            var result = await _auctionRepo.Map_GetAllByAsync<AuctionReturnDto>
            (x => (x.EndAt < DateTime || x.Expired == true) && x.IsDeleted == false);

            return result;
        }
        [AllowAnonymous]

        [HttpGet("getAllEndedByAdminByParent")]
        public async Task<IActionResult> GetAllEndedByAdminByParent()
        {
            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time");
            DateTime DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzi); // convert from utc to local

            var result = await _auctionRepo.Map_GetAllByAsync<AuctionReturnDto>
            (x => (x.EndAt < DateTime || x.Expired == true) && x.IsDeleted == false);
            var x = new { data = result };
            return Ok(x);
        }



        [HttpGet("getAllEndedByProvider")]
        public async Task<IActionResult> GetAllEndedByProvider()
        {
            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time");
            DateTime DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzi); // convert from utc to local

            var result = await _auctionRepo.Map_GetAllByAsync<AuctionReturnDto>
            (x => x.Provider.Id == User.GetUserId() && ((x.EndAt < DateTime || x.Expired == true) && x.IsDeleted == false));

            return Ok(result);
        }


        [AllowAnonymous]

        [HttpGet("getAllActiveByAdmin")]
        public async Task<IEnumerable<AuctionReturnDto>> GetAllActiveByAdmin()

        {

            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time");
            DateTime DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzi); // convert from utc to local

            var result = await _auctionRepo.Map_GetAllByAsync<AuctionReturnDto>
            (x => x.EndAt >= DateTime && x.IsApproved == true && x.Expired == false && x.IsDeleted == false && x.StartAt < DateTime);

            return result;
        }
        [AllowAnonymous]

        [HttpGet("getAllActiveByAdminByParent")]
        public async Task<IActionResult> GetAllActiveByAdminByParent()

        {

            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time");
            DateTime DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzi); // convert from utc to local

            var result = await _auctionRepo.Map_GetAllByAsync<AuctionReturnDto>
            (x => x.EndAt >= DateTime && x.IsApproved == true && x.Expired == false && x.IsDeleted == false && x.StartAt < DateTime);
            var x = new { data = result };
            return Ok(x);
        }
        [AllowAnonymous]

        [HttpGet("getResultByFilter")]
        public async Task<IActionResult> GetResultByFilter(int brand, int model, int carType)

        {

            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time");
            DateTime DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzi); // convert from utc to local

            var result = await _auctionRepo.Map_GetAllByAsync<AuctionReturnDto>
            (x => x.EndAt >= DateTime && x.IsApproved == true && x.Expired == false && x.IsDeleted == false && x.StartAt < DateTime);

            if (brand != 0)
                result = result.Where(x => x.BrandName.Id == brand);

            if (model != 0)
                result = result.Where(x => x.Model.Id == model);

            if (carType != 0)
                result = result.Where(x => x.CarType.Id == carType);


            var x = new { data = result };
            return Ok(x);
        }



        [HttpGet("getAllActiveByProvider")]
        public async Task<IActionResult> GetAllActiveByProvider()
        {

            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time");
            DateTime DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzi); // convert from utc to local

            var result = await _auctionRepo.Map_GetAllByAsync<AuctionReturnDto>
            (x => x.ProviderId == User.GetUserId() && (x.EndAt >= DateTime && x.IsApproved == true && x.Expired == false && x.IsDeleted == false && x.StartAt < DateTime));

            return Ok(result);
        }



        [HttpGet("getAllWaitingByAdmin")]
        public async Task<IActionResult> GetAllWiatingByAdmin()
        {
            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time");
            DateTime DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzi); // convert from utc to local

            var result = await _auctionRepo.Map_GetAllByAsync<AuctionReturnDto>
            (x => x.IsApproved == true && x.IsDeleted == false && x.Expired == false && x.EndAt >= DateTime && x.StartAt > DateTime);

            return Ok(result);
        }



        [HttpGet("getAllWaitingByProvider")]
        public async Task<IActionResult> GetAllWiatingByProvider()
        {
            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time");
            DateTime DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzi); // convert from utc to local

            var result = await _auctionRepo.Map_GetAllByAsync<AuctionReturnDto>
            (x => x.ProviderId == User.GetUserId() && x.IsApproved == true && x.IsDeleted == false && x.EndAt >= DateTime && x.StartAt > DateTime);

            return Ok(result);
        }

        [HttpGet("getAllPendingByAdmin")]
        public async Task<IEnumerable<AuctionReturnDto>> GetAllPendingByAdmin()
        {

            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time");
            DateTime DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzi); // convert from utc to local

            var result = await _auctionRepo.Map_GetAllByAsync<AuctionReturnDto>
            (x => x.IsApproved == false && x.IsDeleted == false && x.EndAt > DateTime);

            return result;
        }

        [HttpGet("getAllPendingByProvider")]
        public async Task<IActionResult> GetAllPendingByProvider()
        {
            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time");
            DateTime DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzi); // convert from utc to local

            var result = await _auctionRepo.Map_GetAllByAsync<AuctionReturnDto>
            (x => x.ProviderId == User.GetUserId() && (x.IsApproved == false && x.IsDeleted == false && x.EndAt > DateTime));

            return Ok(result);
        }
        [AllowAnonymous]
        [HttpGet("GetProviderCount")]

        public async Task<IEnumerable<ProviderReturnDto>> GetProviderCount()
        {

            var provider = await _uow.ProvidersRepo.Map_GetAll<ProviderReturnDto>();

            return provider;
        }
        [AllowAnonymous]
        [HttpGet("GetUsersCount")]

        public async Task<IEnumerable<ClientReturnDto>> GetUsersCount()
        {

            var user = await _uow.ClientRepo.Map_GetAll<ClientReturnDto>();

            return user;
        }
        [AllowAnonymous]
        [HttpGet("GetStatics")]
        public async Task<IActionResult> GetStatics()
        {
            var active = await GetAllActiveByAdmin();
            var pending = await GetAllPendingByAdmin();
            var Ending = await GetAllEndedByAdmin();
            var providers = await GetProviderCount();
            var users = await GetUsersCount();



            StatisticsDto Statics = new StatisticsDto();

            Statics.ActiveAuctions = active.Count();
            Statics.PendingAuctions = pending.Count();
            Statics.EndingAuctions = Ending.Count();
            Statics.Providers = providers.Count();
            Statics.Users = users.Count();
            Statics.Auctions = pending.Count() + Ending.Count() + active.Count();

            return Ok(Statics);
        }
    }
}