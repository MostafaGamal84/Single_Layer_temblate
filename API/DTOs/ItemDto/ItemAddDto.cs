using DTOs;

namespace API.DTOs.ItemDto
{
    public class ItemAddDto : BaseDto
    {
        public int VehicleTypeId { get; set; }
        public int ModelId { get; set; }
        public string PhotoUrl { get; set; }
        public int CarStatusId { get; set; }
        public int ColorId { get; set; }
        public int BrandNameId { get; set; }
        public string OdoMeter { get; set; }
        public string CarNumber { get; set; }
        public string CarId { get; set; }
        public string CarSummary { get; set; }
        public string InitialPrice { get; set; }
        public string SlightestIncrease { get; set; }
        public string ReservePrice { get; set; }
        public int? ProviderId { get; set; }
    }
}