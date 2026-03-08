using System.Collections.Generic;
using API.Entities.Auctions;
using Entities;

namespace API.Entities
{
    public class VehicleType : BaseEntity
    {
        public string Name_ar { get; set; }
        public string Name_en { get; set; }
        public virtual ICollection<Item> Item { get; set; }
    }
}