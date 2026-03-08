using Entities;

namespace API.Entities.Auctions
{
    public class ItemPhoto : BaseEntity
    {
        public string PhotoUrl { get; set; }
        public int ItemId { get; set; }
        public virtual Item Item { get; set; }
    }
}