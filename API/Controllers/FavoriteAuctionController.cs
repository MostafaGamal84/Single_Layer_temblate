
using API.DTOs.FavoriteAuction;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnitOfWork;

namespace API.Controllers
{
    
    public class FavoriteAuctionController : BaseApiController
    {

        private readonly IUnitOfWork _uow;
        private readonly IRepository<FavoriteAuction> _FavoriteRepo;
        public BaseGenericApiController<FavoriteAuction, FavoriteAuctionAddDto, FavoriteAuctionReturnDto> _baseGeneric;

        public FavoriteAuctionController(IUnitOfWork uow)
        {
            _uow = uow;
            _FavoriteRepo = _uow.Repository<FavoriteAuction>();
            _baseGeneric = new BaseGenericApiController<FavoriteAuction, FavoriteAuctionAddDto, FavoriteAuctionReturnDto>(uow);
        }


        [HttpPost("favorite/{id}")]
        public async Task<IActionResult> Add(int id)
        {
            var favorite = _FavoriteRepo.GetBy(x => x.AuctionId == id && x.ClientId == User.GetUserId()) ;

            if (favorite != null)
            {
                favorite.Status = !favorite.Status;

                return await _baseGeneric.Update(favorite);
            }
            else
            {
                var x = new FavoriteAuction();

                x.AuctionId = id;

                x.ClientId = User.GetUserId();

                return await _baseGeneric.Add(x);
            }


        }

        [AllowAnonymous]
        [HttpGet]
        public virtual async Task<IActionResult> Get()
        {
            var result = await _FavoriteRepo.Map_GetAllByAsync<FavoriteAuctionReturnDto>(x => x.IsDeleted == false && x.ClientId == User.GetUserId() && x.Status == true);
            var x = new { data = result };
            return Ok(x);
        }



       

    }
}