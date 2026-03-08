using System.Collections.Generic;
using Entities;

namespace API.Entities.Auctions
{
    public class Item : BaseEntity
    {
        
        public int VehicleTypeId { get; set; }
        public virtual VehicleType VehicleType { get; set; }
        public int ModelId { get; set; }
        public virtual Model Model { get; set; }
        public int CarStatusId { get; set; }
        public virtual CarStatus CarStatus { get; set; }
        public int ColorId { get; set; }
        public virtual Color Color { get; set; }
        public int CityId { get; set; }
        public string City { get; set; }
        public int BrandNameId { get; set; }
        public virtual BrandName BrandName { get; set; }
        public int CarTypeId { get; set; }
        public virtual CarType CarType { get; set; }
        public string OdoMeter { get; set; }
        public string CarNumber { get; set; }
        public string CarId { get; set; }
        public string CarSummary { get; set; }
        public string InitialPrice { get; set; }
        public string ReservePrice { get; set; }
        public string SlightestIncrease { get; set; }
        public string CarReport { get; set; }
        public virtual ICollection<ItemPhoto> itemPhotos { get; set; }
        
    }
}