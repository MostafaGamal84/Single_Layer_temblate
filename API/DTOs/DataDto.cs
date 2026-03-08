using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs.AuctionDto;
using DTOs;

namespace API.DTOs
{
    public class DataDto: BaseDto
    {
        public AuctionReturnDto Data { get; set; }
    }
}