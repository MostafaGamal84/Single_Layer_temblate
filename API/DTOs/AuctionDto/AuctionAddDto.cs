using System;
using API.DTOs.ItemDto;
using DTOs;

namespace API.DTOs.AuctionDto
{
    public class AuctionAddDto : BaseDto
    {


        public bool? Expired { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public int VehicleTypeId { get; set; }
        public int CarTypeId { get; set; }
        public int? ProviderId { get; set; }
        public string City { get; set; }
        public int ModelId { get; set; }
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
        public string CarReportBase64 { get; set; }
        public ICollection<ItemPhotoDto.ItemPhotoAddDto> itemPhotos { get; set; }

    }
}