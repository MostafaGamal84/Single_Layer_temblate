

using DTOs;

namespace API.DTOs.RepairDto
{
    public class RepairReturnDto: BaseDto
    {
        public string Name_ar { get; set; }
        public string Name_en { get; set; }
        public string Descripe { get; set; }
        public string PhotoUrl { get; set; }

    }
}