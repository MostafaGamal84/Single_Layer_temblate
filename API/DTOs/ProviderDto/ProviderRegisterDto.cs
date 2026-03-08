

using System.ComponentModel.DataAnnotations;
using API.DTOs.ProviderPhotoDto;

namespace API.DTOs
{
    public class ProviderRegisterDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string City { get; set; }
        public int ProviderTypeId { get; set; }
        public string Bank { get; set; }
        public string IbanNumber { get; set; }
        public string IdentityNumber { get; set; }
        public string DateOfBirth { get; set; }
        [EmailAddress]
        [Required]
        public string Email { get; set; }
        public string Nationality { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirmation Password is required.")]
        [Compare("Password", ErrorMessage = "Password and Confirmation Password must match.")]
        public string ConfirmPassword { get; set; }
        
        public string BankAccountNumber { get; set; }
        public string AboutCompany { get; set; }
        public string MobileNumber { get; set; }

        public string CompanyServices { get; set; }
        public string CompanyWebSite { get; set; }
        public bool IsApproved { get; set; }
        public bool IsPending { get; set; }

        
        public ProviderPhotoAddDto providerPhotos { get; set; }


    }
}