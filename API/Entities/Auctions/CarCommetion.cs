using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities;

namespace API.Entities.Auctions
{
    public class CarCommetion : BaseEntity
    {
        public int ClientId { get; set; }
        public virtual Client Client { get; set; }
        public int AuctionId { get; set; }
        public virtual Auction Auction { get; set; }
        public double CarReportCommission { get; set; }
    }
}