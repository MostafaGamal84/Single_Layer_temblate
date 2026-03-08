

using DTOs;

namespace API.DTOs.RepairDto
{
    public class RepairAddDto: BaseDto
    {
        public string Name_ar { get; set; }
        public string Name_en { get; set; }
        public string PhotoBase64 { get; set; }
        public string Descripe { get; set; }
   
    }
}