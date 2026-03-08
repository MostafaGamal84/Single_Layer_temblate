using DTOs;

namespace API.DTOs.BannersDto
{
    public class BannersReturnDto : BaseDto
    {
        public string PhotoUrl { get; set; }
        public string Name_ar { get; set; }
        public string Name_en { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
       
    }
}