using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.DTOs
{
    public class StatisticsDto
    {
        public int Providers { get; set; }
        public int Users { get; set; }
        public int Auctions { get; set; }
        public int ActiveAuctions { get; set; }
        public int PendingAuctions { get; set; }
        public int EndingAuctions { get; set; }
    }
}