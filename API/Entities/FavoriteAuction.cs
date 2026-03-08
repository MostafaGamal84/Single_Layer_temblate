using Entities;

namespace API.Entities
{
    public class FavoriteAuction : BaseEntity
    {
        public int ClientId { get; set; }
        public virtual Client Client { get; set; }
        public int AuctionId { get; set; }
        public virtual Auction Auction { get; set; }
       public bool Status { get; set; } = true;
    }
}