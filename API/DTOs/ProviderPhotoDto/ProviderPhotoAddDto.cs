using API.Entities.Auctions;
using DTOs;

namespace API.DTOs.ProviderPhotoDto
{
    public class ProviderPhotoAddDto : BaseDto
    {
        public string CompanyBase64 { get; set; }
        public string CommericalBase64 { get; set; }
        public string IdentityBase64 { get; set; }
    }
}