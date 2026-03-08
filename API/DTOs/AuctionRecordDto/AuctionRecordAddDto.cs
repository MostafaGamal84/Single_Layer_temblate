using API.DTOs.AuctionDto;
using API.Entities;
using DTOs;

namespace API.DTOs.AuctionRecordDto
{
    public class AuctionRecordAddDto : BaseDto
    {
        public int AuctionId { get; set; }
        public double Price { get; set; }
        public double Insurance { get; set; }
        public Double PlusOrMinus { get; set; }
        public Double CarReportCommission { get; set; }

    }
}