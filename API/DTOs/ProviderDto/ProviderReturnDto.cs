
using API.DTOs.ProviderPhotoDto;
using DTOs;

namespace API.DTOs.ProviderDto
{
    public class ProviderReturnDto : BaseDto
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string City { get; set; }
        public string Bank { get; set; }
        public string IbanNumber { get; set; }
        public string MobileNumber { get; set; }
        public string IdentityNumber { get; set; }
        public string DateOfBirth { get; set; }
        public string Email { get; set; }
        public bool IsApproved { get; set; }
        public bool IsPending { get; set; }

        public string Nationality { get; set; }
        public ProviderTypeReturnDto ProviderType { get; set; }
        public string BankAccountNumber { get; set; }
        public string AboutCompany { get; set; }
        public string CompanyServices { get; set; }
        public string CompanyWebSite { get; set; }
        public string Token { get; set; }
        public ProviderPhotoReturnDto providerPhotos { get; set; }
      
    }
}