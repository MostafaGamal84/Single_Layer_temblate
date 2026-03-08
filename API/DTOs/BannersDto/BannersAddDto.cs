using DTOs;

namespace API.DTOs.BannersDto
{
    public class BannersAddDto : BaseDto
    {
        public string FileBase64 { get; set; }
        public string Name_ar { get; set; }
        public string Name_en { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        
    }
}