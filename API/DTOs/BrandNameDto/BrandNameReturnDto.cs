using DTOs;

namespace API.DTOs.BrandNameDto
{
    public class BrandNameReturnDto : BaseDto
    {
        public string Name_ar { get; set; }
        public string Name_en { get; set; }
        public string PhotoUrl { get; set; }
    }
}