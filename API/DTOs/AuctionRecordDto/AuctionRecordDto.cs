using DTOs;

namespace API.DTOs.AuctionRecordDto
{
    public class AuctionRecordDto : BaseDto
    {
        public int AuctionId { get; set; }
        public int ClientId { get; set; }
        public double Price { get; set; }
       

    }
}