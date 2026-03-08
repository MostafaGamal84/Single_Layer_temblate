using DTOs;

namespace API.DTOs.TermsDto
{
    public class TermsAndConditionsReturnDto : BaseDto
    {
        public string Name_ar { get; set; }
        public string Name_en { get; set; }
    }
}