using System.Collections.Generic;
using Entities;

namespace API.Entities.Auctions
{
    public class CarStatus : BaseEntity

    {
        public string Name_ar { get; set; }
        public string Name_en { get; set; }
        public virtual ICollection<Item> Item { get; set; }
    }
}