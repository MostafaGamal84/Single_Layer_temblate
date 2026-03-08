

using DTOs;

namespace API.DTOs.WarrantyDto
{
    public class WarrantyAddDto : BaseDto
    {
      public string Name_ar { get; set; }
      public string Name_en { get; set; }
      public string Descripe { get; set; }
        public string PhotoBase64 { get; set; }
      public string LinkUrl { get; set; }
  
    }
}