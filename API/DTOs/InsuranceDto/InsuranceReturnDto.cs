

using API.DTOs.RepairDto;
using DTOs;

namespace API.DTOs.InsuranceDto
{
    public class InsuranceReturnDto :  BaseDto
    {
        public string Name_ar { get; set; }
        public string Name_en { get; set; }
        public string PhotoUrl { get; set; }

        public string Descripe { get; set; }
    }
}