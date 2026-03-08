using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using API.Entities.Auctions;
using Entities;

namespace API.Entities
{
    [Table("Auction")]
    public class Auction : Item
    {
        public Nullable<int> ProviderId { get; set; }
        public virtual Provider Provider { get; set; }
        public int? AdminId { get; set; }
        public virtual Admin Admin { get; set; }
        public bool Expired { get; set; }
        public bool IsApproved { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public virtual ICollection<AuctionRecord> AuctionRecords { get; set; }
        public virtual ICollection<CarCommetion> CarCommetions { get; set; }
        public virtual ICollection<FavoriteAuction> FavoriteAuctions { get; set; }
    }
}