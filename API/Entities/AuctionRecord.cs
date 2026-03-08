using System;
using System.Collections.Generic;
using Entities;

namespace API.Entities
{
    public class AuctionRecord : BaseEntity
    {
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public Double Price { get; set; }
        public Double Percent { get; set; }
        public Double PlusOrMinus { get; set; }
        public Double AppPercent { get; set; }
        public int ClientId { get; set; }
        public virtual Client Client { get; set; }
        public int AuctionId { get; set; }
        public virtual Auction Auction { get; set; }

    }
}