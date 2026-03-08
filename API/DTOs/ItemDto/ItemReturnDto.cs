using API.DTOs.BrandNameDto;
using API.DTOs.CarStatusDto;
using API.DTOs.ColorDto;
using API.DTOs.ModelDto;
using API.DTOs.ProviderDto;
using API.DTOs.VehicleTypeDto;
using DTOs;

namespace API.DTOs.ItemDto
{
    public class ItemReturnDto : BaseDto
    {
        
        public VehicleTypeReturnDto VehicleType { get; set; }
        public ModelReturnDto Model { get; set; }
        public string PhotoUrl { get; set; }
        public CarStatusReturnDto CarStatus { get; set; }
        public ColorReturnDto Color { get; set; }
        public BrandNameReturnDto BrandName { get; set; }
        public string OdoMeter { get; set; }
        public string CarNumber { get; set; }
        public string CarId { get; set; }
        public string CarSummary { get; set; }
        public string InitialPrice { get; set; }
        public string SlightestIncrease { get; set; }
        
    }
}