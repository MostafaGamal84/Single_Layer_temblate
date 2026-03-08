using API.DTOs.AuctionDto;

namespace API.DTOs.AuctionRecordDto
{
    public class AuctionListDto
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public AuctionReturnDto Auction { get; set; }
    }
}