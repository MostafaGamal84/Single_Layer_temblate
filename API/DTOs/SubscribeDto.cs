using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs.AuctionDto;
using DTOs;

namespace API.DTOs
{
    public class SubscribeDto : BaseDto
    {
        public string Name_ar { get; set; }
        public string Name_en { get; set; }
        public int AuctioningCount { get; set; }
        public double Price { get; set; }
    }
}