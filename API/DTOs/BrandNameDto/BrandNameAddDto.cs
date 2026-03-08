using DTOs;

namespace API.DTOs.BrandNameDto
{
    public class BrandNameAddDto: BaseDto
    {
        public string Name_ar { get; set; }
        public string Name_en { get; set; }
        public string FileBase64 { get; set; }
    }
}