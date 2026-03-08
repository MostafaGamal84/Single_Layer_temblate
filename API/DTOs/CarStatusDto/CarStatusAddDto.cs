using DTOs;

namespace API.DTOs.CarStatusDto
{
    public class CarStatusAddDto : BaseDto
    {
        public string Name_ar { get; set; }
        public string Name_en { get; set; }
    }
}