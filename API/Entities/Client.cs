using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using API.Entities.Auctions;

namespace API.Entities
{
    [Table("Client")]
    public class Client : AppUser
    {

        public string IbanNumber { get; set; }
        public string IdentityNumber { get; set; }
        public string IdentityPhotoUrl { get; set; }
        public string DateOfBirth { get; set; }
        public string Nationality { get; set; }
        public string City { get; set; }
        public int SubscribeCount { get; set; }
        public int? SubscribeId { get; set; } = null;
        public virtual Subscribe Subscribe { get; set; }
        public virtual ICollection<AuctionRecord> AuctionRecords { get; set; }
        public virtual ICollection<CarCommetion> CarCommetions { get; set; }

        public virtual ICollection<FavoriteAuction> FavoriteAuctions { get; set; }

    }
}