using API.DTOs.AuctionDto;
using DTOs;

namespace API.DTOs.AuctionRecordDto
{
    public class AuctionRecordReturnDto : BaseDto
    {
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string ClientName { get; set; }
        public string ClientPhone { get; set; }
        public double Price { get; set; }

        public Double PlusOrMinus { get; set; }
        

    }
}