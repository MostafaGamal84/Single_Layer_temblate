using API.DTOs.AuctionRecordDto;
using API.DTOs.BrandNameDto;
using API.DTOs.CarStatusDto;
using API.DTOs.CarTypeDto;
using API.DTOs.ColorDto;
using API.DTOs.ItemPhotoDto;
using API.DTOs.ModelDto;
using API.DTOs.VehicleTypeDto;
using DTOs;

namespace API.DTOs.EndedAuctionsDto
{
    public class EndedAuctionReturnDto : BaseDto
    {
        public DateTime? EndAt { get; set; }
        public DateTime? StartAt { get; set; }
        public int? TotalRecord { get; set; }
        public string ProviderName { get; set; }
        public string ProviderType { get; set; }
        public double? SellerAmount { get; set; }
        public double? Insurance { get; set; }
        public string CarIdNumber { get; set; }
        public double? AppPercent { get; set; }
        public double? FinalTotal { get; set; }
        public double? Percent { get; set; }
        public double? Price { get; set; }
        public Double PlusOrMinus { get; set; }
        public int AuctionId { get; set; }
        public ICollection<ItemPhotoReturnDto> itemPhotos { get; set; }
      
    }
}