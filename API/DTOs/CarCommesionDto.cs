using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs.AuctionDto;
using DTOs;

namespace API.DTOs
{
    public class CarCommesionDto : BaseDto
    {
        public int AuctionId { get; set; }
        public AuctionReturnDto Auction { get; set; }

        public int ClientId { get; set; }
        public ClientReturnDto Client { get; set; }
        public double CarReportCommission { get; set; }

    }
}