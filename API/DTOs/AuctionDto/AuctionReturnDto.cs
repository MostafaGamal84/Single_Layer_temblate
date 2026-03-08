using API.DTOs.AdminDto;
using API.DTOs.BrandNameDto;
using API.DTOs.CarStatusDto;
using API.DTOs.CarTypeDto;
using API.DTOs.ColorDto;
using API.DTOs.ItemDto;
using API.DTOs.ItemPhotoDto;
using API.DTOs.ModelDto;
using API.DTOs.ProviderDto;
using API.DTOs.VehicleTypeDto;
using DTOs;

namespace API.DTOs.AuctionDto
{
    public class AuctionReturnDto : BaseDto
    {

        public int? ProviderId { get; set; }
        public ProviderReturnDto Provider { get; set; }
        public string ClientName { get; set; }
        public string ProviderName { get; set; }
        public int? AdminId { get; set; }
        public AdminReturnDto Admin { get; set; }
        public bool IsStarted { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public VehicleTypeReturnDto VehicleType { get; set; }
        public ModelReturnDto Model { get; set; }
        public string City { get; set; }
        public CarStatusReturnDto CarStatus { get; set; }
        public ColorReturnDto Color { get; set; }
        public BrandNameReturnDto BrandName { get; set; }
        public CarTypeReturnDto CarType { get; set; }
        public string OdoMeter { get; set; }
        public string CarNumber { get; set; }
        public string CarId { get; set; }
        public string CarSummary { get; set; }
        public string TotalRecord { get; set; }
        public string CarReport { get; set; }
        public string Price { get; set; }
        public string InitialPrice { get; set; }
        public string SlightestIncrease { get; set; }
        public bool IsFavorite { get; set; } = false;
        public ICollection<ItemPhotoReturnDto> itemPhotos { get; set; }

    }
}