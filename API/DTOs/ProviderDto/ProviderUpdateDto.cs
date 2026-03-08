using System.ComponentModel.DataAnnotations;
using API.DTOs.ProviderPhotoDto;

namespace API.DTOs.ProviderDto
{
    public class ProviderUpdateDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string City { get; set; }
        public int ProviderTypeId { get; set; }
        public string IdentityNumber { get; set; }
        public string Email { get; set; }
        public string Nationality { get; set; }
        public string BankAccountNumber { get; set; }
        public string IbanNumber { get; set; }
        public string MobileNumber { get; set; }
         public string AboutCompany { get; set; }
        public string CompanyServices { get; set; }
        public string CompanyWebSite { get; set; }
        public bool IsApproved { get; set; }
        public bool IsPending { get; set; }

        public ProviderPhotoAddDto providerPhotos { get; set; }
    }
}