using API.DTOs.AuctionDto;
using DTOs;

namespace API.DTOs.FavoriteAuction
{
    public class FavoriteAuctionReturnDto : BaseDto
    {
          public int AuctionId { get; set; }
          public AuctionReturnDto Auction{ get; set; }
    }
}