

using DTOs;

namespace API.DTOs.WarrantyDto
{
    public class WarrantyReturnDto : BaseDto
    {
        public string Name_ar { get; set; }
        public string Name_en { get; set; }
        public string PhotoUrl { get; set; }

        public string Descripe { get; set; }
        public string LinkUrl { get; set; }
        
    }
}